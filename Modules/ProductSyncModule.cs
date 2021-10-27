using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules {
    class ProductSyncModule {

        public static string BaseUrl = "https://real-europe-corp.myshopify.com/admin/api/2021-10/";
        public static string ProductPath = "products.json";

        public List<SyncProduct> syncProducts;
        private readonly RESTClient ProductClient;

        Dictionary<string, string> mainHeaders = new Dictionary<string, string>(){
                {"Authorization", Globals.getBase64ShopifyCreds()}
            };

        public ProductSyncModule() {
            syncProducts = GetSyncProducts();
            ProductClient = new(BaseUrl);
        }

        private List<SyncProduct> GetSyncProducts(string status = null) {
            List<SyncProduct> p = new();
            DataBaseInterface db = new();
            Dictionary<string, Dictionary<string, string>> whereCond;

            if (status == null) {
                whereCond = new Dictionary<string, Dictionary<string, string>> {
                    ["Status"] = new Dictionary<string, string> {
                        ["!="] = ProductStatus.Ok
                    }
                };
            } else {
                whereCond = new Dictionary<string, Dictionary<string, string>> {
                    ["Status"] = new Dictionary<string, string> {
                        ["="] = status
                    }
                };
            }

            var result = db.Table("Products").Where(whereCond).Get();

            foreach (var row in result.Values) {
                SyncProduct product = new();

                product.id = int.Parse(row["ID"]);


                product.sku = row["SKU"];
                product.status = row["Status"];
                product.lastSyncTime = row["LastSyncTime"].UnixTimeToSrt();
                product.lastUpdateTime = row["LastUpdateTime"].UnixTimeToSrt();
                product.shopifyID = row["ShopifyID"];
                p.Add(product);
            }
            return p;
        }

        public void ExportShopifyProducts(object sender, DoWorkEventArgs e) {
            List<SyncProduct> syncProducts = GetSyncProducts();
            int count = syncProducts.Count;
            for (int i = 0; i < count; i++) {

                int progress = i * 1000 / count;
                (sender as BackgroundWorker).ReportProgress(progress);
                ExportShopifyProduct(syncProducts[i]);
            }
        }

        //method decides what to do with syncProduct
        private void ExportShopifyProduct(SyncProduct sync) {
            ProductModule pm = new();
            Product p = pm.GetProduct(sync.sku);

            if (sync.status == ProductStatus.New) {
                NewShopifyProduct(p, sync);
            } else if (sync.status == ProductStatus.WaitingShopSync) {
                //todo: product update
            } else {
                throw new Exception($"No Product Status - {sync.status}");
            }
        }

        private void NewShopifyProduct(Product p, SyncProduct sync) {

            //adding product to shopify
            string creationRes = ProductClient.ExecPostProd(ProductPath, mainHeaders, p.GetImportJsonString(), sync);
            if (creationRes == "503 and DeleteFail") {
                NewShopifyProduct(p, sync);
                return;
            }

            dynamic data = JsonConvert.DeserializeObject<dynamic>(creationRes);
            string shopifyID = data["product"]["variants"].First["product_id"].Value.ToString();
            string shopifyVariantID = data["product"]["variants"].First["id"].Value.ToString();
            string inventoryItemID = data["product"]["variants"].First["inventory_item_id"].Value.ToString();

            sync.shopifyID = shopifyID;
            sync.shopifyVariantID = shopifyVariantID;
            sync.inventoryItemID = inventoryItemID;

            //adding dimensions
            bool heightRes = ProductClient.ExecPostProdBool($"products/{shopifyID}/metafields.json", mainHeaders, p.GetHeightMetaBody(), sync);
            if (heightRes == false) {
                NewShopifyProduct(p, sync);
                return;
            }

            bool lenghtRes = ProductClient.ExecPostProdBool($"products/{shopifyID}/metafields.json", mainHeaders, p.GetLenghtMetaBody(), sync);
            if (lenghtRes == false) {
                NewShopifyProduct(p, sync);
                return;
            }

            bool widthRes = ProductClient.ExecPostProdBool($"products/{shopifyID}/metafields.json", mainHeaders, p.GetWidthMetaBody(), sync);
            if (widthRes == false) {
                NewShopifyProduct(p, sync);
                return;
            }

            //adding vendor price
            bool vendorPriceRes = ProductClient.ExecPutProd($"inventory_items/{inventoryItemID}.json", mainHeaders, p.GetVendorPriceBody(), sync);
            if (vendorPriceRes == false) {
                NewShopifyProduct(p, sync);
                return;
            }

            //adding new shopify id to
            DataBaseInterface db = new();
            var updateData = new Dictionary<string, string> {
                ["ShopifyID"] = sync.shopifyID,
                ["ShopifyVariantID"] = sync.shopifyVariantID,
                ["ShopifyInventoryItemID"] = sync.inventoryItemID,
                ["Status"] = "Ok",
                ["LastSyncTime"] = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString()
            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>> {
                ["SKU"] = new Dictionary<string, string> {
                    ["="] = sync.sku
                }
            };
            db.Table("Products").Where(whereUpdate).Update(updateData);

        }
    }
}
