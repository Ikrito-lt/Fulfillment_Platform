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
using RestSharp;
using System.Threading;

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

        public List<SyncProduct> RefreshSyncProducts() {
            syncProducts = GetSyncProducts();
            return syncProducts;
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
                product.inventoryItemID = row["ShopifyInventoryItemID"];
                product.shopifyVariantID = row["ShopifyVariantID"];
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
            Product p = ProductModule.GetProduct(sync.sku);

            if (sync.status == ProductStatus.New) {
                NewShopifyProduct(p, sync);
            } else if (sync.status == ProductStatus.WaitingShopSync) {
                UpdateShopifyProduct(p, sync);
            } else {
                throw new Exception($"No Product Status - {sync.status}");
            }
        }

        private void UpdateShopifyProduct(Product p, SyncProduct sync) {
            //updating product in shopify
            IRestResponse updateRes = ProductClient.ExecPutProd($"products/{sync.shopifyID}.json", mainHeaders, p.GetImportJsonString());
            if (!updateRes.IsSuccessful) {
                if (updateRes.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                    Thread.Sleep(5000);

                    UpdateShopifyProduct(p, sync);
                    return;
                } else {
                    throw updateRes.ErrorException;
                }
            }

            dynamic data = JsonConvert.DeserializeObject<dynamic>(updateRes.Content);
            string shopifyVariantID = data["product"]["variants"].First["id"].Value.ToString();
            string inventoryItemID = data["product"]["variants"].First["inventory_item_id"].Value.ToString();

            sync.shopifyVariantID = shopifyVariantID;
            sync.inventoryItemID = inventoryItemID;

            //to update metafields i need to get metafield IDs
            string getMetaRes = ProductClient.ExecGet($"products/{sync.shopifyID}/metafields.json", mainHeaders);
            var ids = ExtractMetafieldIDs(getMetaRes);

            //updating height metafield
            if (ids.ContainsKey("dimensions")) {
                string heightVal = $"\"{{\\\"unit\\\": \\\"cm\\\",\\\"value\\\": {p.height}}}\"";
                string heightBody = $@"{{""metafield"": {{""value"": {heightVal} }}}}";
                IRestResponse heightRes = ProductClient.ExecPutProd($"products/{sync.shopifyID}/metafields/{ids["dimensions"]}.json", mainHeaders, heightBody);
                if (!heightRes.IsSuccessful) {
                    if (heightRes.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                        Thread.Sleep(5000);

                        UpdateShopifyProduct(p, sync);
                        return;
                    } else {
                        throw heightRes.ErrorException;
                    }
                }
            } else {
                throw new Exception($"SKU: {sync.sku} >> UpdateFail no heightMeta");
            }

            //updating lenght metafield
            if (ids.ContainsKey("lenght")) {
                string lenghtVal = $"\"{{\\\"unit\\\": \\\"cm\\\",\\\"value\\\": {p.lenght}}}\"";
                string lenghtBody = $@"{{""metafield"": {{""value"": {lenghtVal} }}}}";
                IRestResponse lenghtRes = ProductClient.ExecPutProd($"products/{sync.shopifyID}/metafields/{ids["lenght"]}.json", mainHeaders, lenghtBody);
                if (!lenghtRes.IsSuccessful) {
                    if (lenghtRes.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                        Thread.Sleep(5000);

                        UpdateShopifyProduct(p, sync);
                        return;
                    } else {
                        throw lenghtRes.ErrorException;
                    }
                }
            } else {
                throw new Exception($"SKU: {sync.sku} >> UpdateFail no lenghtMeta");
            }

            //updating width metafield
            if (ids.ContainsKey("width")) {
                string widthVal = $"\"{{\\\"unit\\\": \\\"cm\\\",\\\"value\\\": {p.width}}}\"";
                string widthBody = $@"{{""metafield"": {{""value"": {widthVal} }}}}";
                IRestResponse widthRes = ProductClient.ExecPutProd($"products/{sync.shopifyID}/metafields/{ids["width"]}.json", mainHeaders, widthBody);
                if (!widthRes.IsSuccessful) {
                    if (widthRes.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                        Thread.Sleep(5000);

                        UpdateShopifyProduct(p, sync);
                        return;
                    } else {
                        throw widthRes.ErrorException;
                    }
                }
            } else {
                throw new Exception($"SKU: {sync.sku} >> UpdateFail no widthMeta");
            }

            //updating vendor price
            IRestResponse vendorPriceRes = ProductClient.ExecPutProd($"inventory_items/{sync.inventoryItemID}.json", mainHeaders, p.GetVendorPriceBody());
            if (!vendorPriceRes.IsSuccessful) {
                if (vendorPriceRes.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                    Thread.Sleep(5000);

                    UpdateShopifyProduct(p, sync);
                    return;
                } else {
                    throw vendorPriceRes.ErrorException;
                }
            }

            //updating product status
            DataBaseInterface db = new();
            var updateData = new Dictionary<string, string> {
                ["Status"] = ProductStatus.Ok,
                ["LastSyncTime"] = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString()
            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>> {
                ["SKU"] = new Dictionary<string, string> {
                    ["="] = sync.sku
                }
            };
            db.Table("Products").Where(whereUpdate).Update(updateData);

        }

        private void NewShopifyProduct(Product p, SyncProduct sync) {

            //adding product to shopify
            IRestResponse creationRes = ProductClient.ExecAddProd(ProductPath, mainHeaders, p.GetImportJsonString());
            if (!creationRes.IsSuccessful) {
                if (creationRes.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                    Thread.Sleep(5000);

                    bool deleted = ProductClient.ExecDeleteProd(mainHeaders, sync.shopifyID);
                    if (deleted == false) {
                        throw new Exception($"SKU: {sync.sku} >> 503 and DeleteFail (NewShopifyProduct createProd)");
                    } else {
                        NewShopifyProduct(p, sync);
                        return;
                    }
                } else {
                    throw creationRes.ErrorException;
                }
            }

            dynamic data = JsonConvert.DeserializeObject<dynamic>(creationRes.Content);
            string shopifyID = data["product"]["variants"].First["product_id"].Value.ToString();
            string shopifyVariantID = data["product"]["variants"].First["id"].Value.ToString();
            string inventoryItemID = data["product"]["variants"].First["inventory_item_id"].Value.ToString();

            sync.shopifyID = shopifyID;
            sync.shopifyVariantID = shopifyVariantID;
            sync.inventoryItemID = inventoryItemID;

            //adding height metafield
            IRestResponse heightRes = ProductClient.ExecAddProd($"products/{shopifyID}/metafields.json", mainHeaders, p.GetHeightMetaBody());
            if (!heightRes.IsSuccessful) {
                if (heightRes.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                    Thread.Sleep(5000);

                    bool deleted = ProductClient.ExecDeleteProd(mainHeaders, sync.shopifyID);
                    if (deleted == false) {
                        throw new Exception($"SKU: {sync.sku} >> 503 and DeleteFail (NewShopifyProduct height)");
                    } else {
                        NewShopifyProduct(p, sync);
                        return;
                    }
                } else {
                    throw heightRes.ErrorException;
                }
            }

            //adding lenght metafield
            IRestResponse lenghtRes = ProductClient.ExecAddProd($"products/{shopifyID}/metafields.json", mainHeaders, p.GetLenghtMetaBody());
            if (!lenghtRes.IsSuccessful) {
                if (lenghtRes.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                    Thread.Sleep(5000);

                    bool deleted = ProductClient.ExecDeleteProd(mainHeaders, sync.shopifyID);
                    if (deleted == false) {
                        throw new Exception($"SKU: {sync.sku} >> 503 and DeleteFail (NewShopifyProduct lenght)");
                    } else {
                        NewShopifyProduct(p, sync);
                        return;
                    }
                } else {
                    throw lenghtRes.ErrorException;
                }
            }

            //adding width metafield
            IRestResponse widthRes = ProductClient.ExecAddProd($"products/{shopifyID}/metafields.json", mainHeaders, p.GetWidthMetaBody());
            if (!widthRes.IsSuccessful) {
                if (widthRes.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                    Thread.Sleep(5000);

                    bool deleted = ProductClient.ExecDeleteProd(mainHeaders, sync.shopifyID);
                    if (deleted == false) {
                        throw new Exception($"SKU: {sync.sku} >> 503 and DeleteFail (NewShopifyProduct width)");
                    } else {
                        NewShopifyProduct(p, sync);
                        return;
                    }
                } else {
                    throw widthRes.ErrorException;
                }
            }

            //adding vendor price
            IRestResponse vendorPriceRes = ProductClient.ExecPutProd($"inventory_items/{inventoryItemID}.json", mainHeaders, p.GetVendorPriceBody());
            if (!vendorPriceRes.IsSuccessful) {
                if (vendorPriceRes.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                    Thread.Sleep(5000);

                    bool deleted = ProductClient.ExecDeleteProd(mainHeaders, sync.shopifyID);
                    if (deleted == false) {
                        throw new Exception($"SKU: {sync.sku} >> 503 and DeleteFail (NewShopifyProduct vendorPrice)");
                    } else {
                        NewShopifyProduct(p, sync);
                        return;
                    }
                } else {
                    throw vendorPriceRes.ErrorException;
                }
            }

            //adding new shopify id to
            DataBaseInterface db = new();
            var updateData = new Dictionary<string, string> {
                ["ShopifyID"] = sync.shopifyID,
                ["ShopifyVariantID"] = sync.shopifyVariantID,
                ["ShopifyInventoryItemID"] = sync.inventoryItemID,
                ["Status"] = ProductStatus.Ok,
                ["LastSyncTime"] = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString()
            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>> {
                ["SKU"] = new Dictionary<string, string> {
                    ["="] = sync.sku
                }
            };
            db.Table("Products").Where(whereUpdate).Update(updateData);

        }

        public Dictionary<string, string> ExtractMetafieldIDs(string json) {

            Dictionary<string, string> ids = new();

            dynamic dJson = JsonConvert.DeserializeObject(json);
            var metaFields = dJson["metafields"];

            foreach (var field in metaFields) {
                string key = Convert.ToString(field["key"]);
                string id = Convert.ToString(field["id"]);
                string nameSpace = Convert.ToString(field["namespace"]);

                if (nameSpace == "my_fields") {
                    if (key == "lenght" || key == "dimensions" || key == "width") {
                        ids.Add(key, id);
                    }
                }
            }
            return ids;
        }
    }
}
