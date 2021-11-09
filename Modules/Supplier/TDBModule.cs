using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Linq;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using Ikrito_Fulfillment_Platform.Models;

namespace Ikrito_Fulfillment_Platform.Modules {
    class TDBModule {

        private static Dictionary<string, string> _APIParams = new Dictionary<string, string> {
            ["orgnum"] = "268230",
            ["username"] = "PREKES",
            ["pwd"] = "Welcome.123",
            ["ean"] = "y"
        };

        //private readonly string TDBDesc_location = @"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Files\TDB\TDB_cat.xml";
        //private readonly string TDBCat_location = @"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Files\TDB\TDB_cat2.xml";
        //private readonly string TDBCategoriesJson = @"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Files\TDB\TDBCategories.json";

        private static string _BaseUrl = "http://tdonline.tdbaltic.net/pls/PROD/";
        private static string _CataloguePath = "ixml.ProdCatExt";
        private static string _DataSheetsPath = "ixml.DSheets";
        private static string _DBTablePrefix = "TDB_";
        private static string _SKUPrefix = "TDB-";

        private Lazy<XmlDocument> _LazyDataSheetXML = new Lazy<XmlDocument>(() => GetTDBDataSheets());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private XmlDocument _DataSheetsXML => _LazyDataSheetXML.Value;

        private Lazy<XmlDocument> _LazyCategoryXML = new Lazy<XmlDocument>(() => GetTDBCatalogue());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private XmlDocument _CategoryXML => _LazyCategoryXML.Value;

        //downloads Catalogue from TDB API
        private static XmlDocument GetTDBCatalogue() {
            RESTClient restClient = new(_BaseUrl);
            string xmlCatalogueStr = restClient.ExecGetParams(_CataloguePath, _APIParams);

            XmlDocument categoryXML = new();
            categoryXML.LoadXml(xmlCatalogueStr);

            return categoryXML;
        }

        //downloads Catalogue from TDB API
        private static XmlDocument GetTDBDataSheets() {
            Dictionary<string, string> dataSheetParams = _APIParams;
            dataSheetParams.Remove("ean"); 

            RESTClient restClient = new(_BaseUrl);
            string xmlDataSheetStr = restClient.ExecGetParams(_DataSheetsPath, dataSheetParams);

            XmlDocument dataSheetXML = new();
            dataSheetXML.LoadXml(xmlDataSheetStr);

            return dataSheetXML;
        }

        public void updateTDBProducts(object sender = null, DoWorkEventArgs e = null) {
            List<Dictionary<string, string>> pendingChanges = new();
            Dictionary<string, Dictionary<string, string>> appliedChanges = new();

            var catalogueProducts = _CategoryXML.FirstChild.ChildNodes;
            foreach (XmlNode prodXML in catalogueProducts) {
                Dictionary<string, string> productInfo = new();

                XmlNode skuNode = prodXML.SelectSingleNode("TDPartNbr");
                XmlNode priceVendorNode = prodXML.SelectSingleNode("Price");
                XmlNode stockNode = prodXML.SelectSingleNode("Stock");
                XmlNode barcodeNode = prodXML.SelectSingleNode("Ean");
                XmlNode vendorNode = prodXML.SelectSingleNode("Manuf");

                productInfo.Add("SKU", "TDB-" + skuNode.InnerText);
                productInfo.Add("PriceVendor", priceVendorNode.InnerText);
                productInfo.Add("Stock", stockNode.InnerText);
                productInfo.Add("Barcode", barcodeNode.InnerText);
                productInfo.Add("Vendor", vendorNode.InnerText);

                pendingChanges.Add(productInfo);
            }

            //getting sku product from sku table Product table
            DataBaseInterface db = new();
            var productTable = db.Table("Products").Get();
            var products = productTable.Values.ToList();
            products.RemoveAll(x => !x["SKU"].StartsWith("TDB-"));

            foreach (var product in products) {
                string sku = product["SKU"];
                var productChanges = pendingChanges.Find(x => x["SKU"] == sku);

                //handles archiving of products if tdb doesnt sell it anymore
                if (productChanges == null) {
                    
                    //mark this product for archiving
                    var updateData = new Dictionary<string, string> {
                        ["Status"] = ProductStatus.NeedsArchiving,
                        ["LastUpdateTime"] = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString()
                    };
                    var whereUpdate = new Dictionary<string, Dictionary<string, string>> {
                        ["SKU"] = new Dictionary<string, string> {
                            ["="] = sku
                        }
                    };
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                    continue;
                } else {
                    
                    //get product from db 
                    var whereQuery = new Dictionary<string, Dictionary<string, string>> {
                        ["SKU"] = new Dictionary<string, string> {
                            ["="] = sku
                        }
                    };
                    var oldProductDB = db.Table("TDB_Products").Where(whereQuery).Get()[0];

                    //if data is the same dont change shit 
                    if (oldProductDB["Stock"] == productChanges["Stock"] && oldProductDB["PriceVendor"] == productChanges["PriceVendor"]) {
                        pendingChanges.RemoveAll(x => x["SKU"] == sku);
                        continue;
                    } else {
                        appliedChanges.Add(sku, new Dictionary<string, string>());
                        //updating stock value
                        if (oldProductDB["Stock"] != productChanges["Stock"]) {
                            var stockUpdateData = new Dictionary<string, string> {
                                ["Stock"] = productChanges["Stock"]
                            };
                            var stockWhereUpdate = new Dictionary<string, Dictionary<string, string>> {
                                ["SKU"] = new Dictionary<string, string> {
                                    ["="] = sku
                                }
                            };
                            db.Table("TDB_Products").Where(stockWhereUpdate).Update(stockUpdateData);

                            //adding change to applied change list
                            appliedChanges[sku].Add("Stock", $"{oldProductDB["Stock"]} -> {productChanges["Stock"]}");
                        }

                        if (oldProductDB["PriceVendor"] != productChanges["PriceVendor"]) {
                            double priceChangeAmmount = double.Parse(productChanges["PriceVendor"]) - double.Parse(oldProductDB["PriceVendor"]);
                            int priceChangeRounded = Convert.ToInt32(priceChangeAmmount);
                            double newSalePrice = double.Parse(oldProductDB["Price"]) + priceChangeRounded;

                            //updating price value
                            var priceUpdateData = new Dictionary<string, string> {
                                ["PriceVendor"] = productChanges["PriceVendor"],
                                ["Price"] = newSalePrice.ToString()
                            };
                            var priceWhereUpdate = new Dictionary<string, Dictionary<string, string>> {
                                ["SKU"] = new Dictionary<string, string> {
                                    ["="] = sku
                                }
                            };
                            db.Table("TDB_Products").Where(priceWhereUpdate).Update(priceUpdateData);

                            //adding change to applied change list
                            appliedChanges[sku].Add("PriceVendor", $"{oldProductDB["PriceVendor"]} -> {productChanges["PriceVendor"]}");
                            appliedChanges[sku].Add("Price", $"{oldProductDB["Price"]} -> {newSalePrice}");
                        }

                        //marking product for shop sync
                        var statusUpdateData = new Dictionary<string, string> {
                            ["Status"] = ProductStatus.WaitingShopSync,
                            ["LastUpdateTime"] = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString()
                        };
                        var statusWhereUpdate = new Dictionary<string, Dictionary<string, string>> {
                            ["SKU"] = new Dictionary<string, string> {
                                ["="] = sku
                            }
                        };
                        db.Table("Products").Where(statusWhereUpdate).Update(statusUpdateData);
                    }

                    //remove entry from change list
                    pendingChanges.RemoveAll(x => x["SKU"] == sku);
                }
            }

            //adding new products to database
            List<Dictionary<string, string>> newProducts = new();
            foreach (var nTDBProduct in pendingChanges) {
                bool newProductWasAdded = addNewTDBProduct(nTDBProduct);

                if (newProductWasAdded) {
                    //adding product to this list to mark it as newly added
                    newProducts.Add(nTDBProduct);
                    //removing product from pendingChanges list
                    pendingChanges.Remove(nTDBProduct);
                }
            }

            //pass applied changes and pending changes to update on complete method
            Dictionary<string, object> changes = new();
            changes.Add("appliedChanges", appliedChanges);
            changes.Add("pendingChanges", pendingChanges);
            changes.Add("newProducts", newProducts);
            if (e != null) {
                e.Result = changes;
            }
        }

        public void updateTDBProductsComplete(object sender, RunWorkerCompletedEventArgs e) {

        }

        //todo:change to private
        public bool addNewTDBProduct(Dictionary<string, string> newProductKVP) { 
            bool productAdded = false;
            string newProdSKU = newProductKVP["SKU"];
            string newProdTDBSKU = newProdSKU.Substring(newProdSKU.IndexOf('-') + 1);

            var newProdData = _DataSheetsXML.SelectSingleNode(@$"/Datasheets/Datasheet[@TDPartNbr='{newProdTDBSKU}']");
            Product newProduct = new();

            newProduct.title = newProdData.SelectSingleNode("/ShortDesc").InnerText;
            newProduct.vendor = newProductKVP["Vendor"];
            newProduct.product_type = "Not-Assigned";
            //newProduct.price
            //newProduct.body_html
            newProduct.sku = newProdSKU;
            newProduct.stock = int.Parse(newProductKVP["Stock"]);
            newProduct.barcode = newProductKVP["Barcode"];
            newProduct.vendor_price = double.Parse(newProductKVP["PriceVendor"]);


            //getting weight
            double grossWeight = double.Parse();

            newProduct.weight = double.Parse()



            return productAdded;
        }

        //todo: add needs archiving clause to sync products module



    }
}
