using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Xml;

namespace Ikrito_Fulfillment_Platform.Modules {
    class TDBModule {

        private static Dictionary<string, string> _APIParams = new Dictionary<string, string> {
            ["orgnum"] = "268230",
            ["username"] = "PREKES",
            ["pwd"] = "Welcome.123",
            ["ean"] = "y"
        };

        private List<string> descSkipableKeys = new List<string>(){
                "Manufacturer Logo",
                "Picture1",
                "Picture2",
                "Picture3",
                "Picture4",
                "LongDesc",
                "ShortDesc",
                "Marketing Text"
        };

        private static string _BaseUrl = "http://tdonline.tdbaltic.net/pls/PROD/";
        private static string _CataloguePath = "ixml.ProdCatExt";
        private static string _DataSheetsPath = "ixml.DSheets";

        private Lazy<XmlDocument> _LazyDataSheetXML = new Lazy<XmlDocument>(() => GetTDBDataSheets());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private XmlDocument _DataSheetsXML => _LazyDataSheetXML.Value;

        private Lazy<XmlDocument> _LazyCategoryXML = new Lazy<XmlDocument>(() => GetTDBCatalogue());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private XmlDocument _CategoryXML => _LazyCategoryXML.Value;


        //
        // section of methods for getting data from TDB API
        //

        //downloads product catalogue from TDB API
        private static XmlDocument GetTDBCatalogue() {
            RESTClient restClient = new(_BaseUrl);
            string xmlCatalogueStr = restClient.ExecGetParams(_CataloguePath, _APIParams);

            XmlDocument categoryXML = new();
            categoryXML.LoadXml(xmlCatalogueStr);

            return categoryXML;
        }

        //downloads product datasheets from TDB API
        private static XmlDocument GetTDBDataSheets() {
            Dictionary<string, string> dataSheetParams = _APIParams;
            dataSheetParams.Remove("ean");

            RESTClient restClient = new(_BaseUrl);
            string xmlDataSheetStr = restClient.ExecGetParams(_DataSheetsPath, dataSheetParams);

            XmlDocument dataSheetXML = new();
            dataSheetXML.LoadXml(xmlDataSheetStr);

            return dataSheetXML;
        }


        //
        // Section for automatically updating and adding product to database
        //
        
        // Updates product and then sends products that dont exist to addNewProduct method
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
                XmlNode vendorTypeNode = prodXML.SelectSingleNode("SubClassCode");

                productInfo.Add("SKU", "TDB-" + skuNode.InnerText);
                productInfo.Add("PriceVendor", priceVendorNode.InnerText);
                productInfo.Add("Stock", stockNode.InnerText);
                productInfo.Add("Barcode", barcodeNode.InnerText);
                productInfo.Add("Vendor", vendorNode.InnerText);
                productInfo.Add("VendorType", vendorTypeNode.InnerText);

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
                    ProductModule.ChangeProductStatus(sku, ProductStatus.NeedsArchiving);
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
                            //todo: impelelent price variator to product prices

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

                        ProductModule.ChangeProductStatus(sku, ProductStatus.WaitingShopSync);
                    }

                    //remove entry from change list
                    pendingChanges.RemoveAll(x => x["SKU"] == sku);
                }
            }

            //adding new products to database
            List<Dictionary<string, string>> newProducts = new();
            List<Dictionary<string, string>> ProductsToAdd = new(pendingChanges);
            foreach (var nTDBProduct in ProductsToAdd) {
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

        // adds new TDB product to database
        private bool addNewTDBProduct(Dictionary<string, string> newProductKVP) {
            bool productAdded = false;
            string newProdSKU = newProductKVP["SKU"];
            string newProdTDBSKU = newProdSKU.Substring(newProdSKU.IndexOf('-') + 1);

            var newProdDataXML = _DataSheetsXML.SelectSingleNode(@$"/Datasheets/Datasheet[@TDPartNbr='{newProdTDBSKU}']");
            if (newProdDataXML == null) {
                return productAdded;
            } else {
                Dictionary<string, string> newProdDataKVP = GetProductDataKVP(newProdDataXML);

                //init new product object
                Product newProduct = new();

                string title = newProdDataKVP["ShortDesc"];
                title = title.Replace("'", "''");

                newProduct.title = title;
                newProduct.vendor = newProductKVP["Vendor"];
                newProduct.product_type = "Not-Assigned";
                newProduct.sku = newProdSKU;
                newProduct.stock = int.Parse(newProductKVP["Stock"]);
                newProduct.barcode = newProductKVP["Barcode"];
                newProduct.vendor_price = double.Parse(newProductKVP["PriceVendor"]);

                //getting weight
                string grossWeightStr = "0";
                bool grossExists = newProdDataKVP.TryGetValue("Gross Weight", out grossWeightStr);
                string netWeightStr = "0";
                bool netExists = newProdDataKVP.TryGetValue("Net Weight", out grossWeightStr);

                if (!grossExists) { grossWeightStr = "0"; }
                if (!netExists) { netWeightStr = "0"; }

                grossWeightStr = grossWeightStr.Split(" ")[0];
                netWeightStr = netWeightStr.Split(" ")[0];

                double grossWeight = .0;
                double netWeight = .0;

                bool grossWeightConvSucceded = double.TryParse(grossWeightStr, out grossWeight);
                if (!grossWeightConvSucceded) {grossWeight = .0;}
                bool netWeightConvSucceded = double.TryParse(netWeightStr, out netWeight);
                if (!netWeightConvSucceded) { netWeight = .0; }
                newProduct.weight = Math.Max(grossWeight, netWeight);

                //getting height
                string heightStr = "0";
                bool heightExists = newProdDataKVP.TryGetValue("Height", out heightStr);
                if (!heightExists) { heightStr = "0"; }

                heightStr = heightStr.Split(" ")[0];
                int heightInt = 0;
                bool heightConvSucceded = int.TryParse(heightStr, out heightInt);
                if (heightConvSucceded) {
                    newProduct.height = heightInt;
                } else {
                    newProduct.height = 0;
                }

                //getting lenght
                string lenghtStr = "0";
                bool lenghtExists = newProdDataKVP.TryGetValue("Lenght", out lenghtStr);
                if (!lenghtExists) { lenghtStr = "0"; }

                lenghtStr = lenghtStr.Split(" ")[0];
                int lenghtInt = 0;
                bool lenghtConvSucceded = int.TryParse(lenghtStr, out lenghtInt);
                if (lenghtConvSucceded) {
                    newProduct.lenght = lenghtInt;
                } else {
                    newProduct.lenght = 0;
                }

                //getting width
                string widthStr = "0";
                bool widthExists = newProdDataKVP.TryGetValue("Width", out widthStr);
                if (!widthExists) { widthStr = "0"; }

                widthStr = widthStr.Split(" ")[0];
                int widthInt = 0;
                bool widthConvSucceded = int.TryParse(widthStr, out widthInt);
                if (widthConvSucceded) {
                    newProduct.width = widthInt;
                } else {
                    newProduct.width = 0;
                }

                //calvulating newProduct price with 15-30% margin choosen on random
                int marginPercent = new Random().Next(15, 30);
                double newPrice = (newProduct.vendor_price * (100 + marginPercent)) / 100;
                newPrice = Math.Floor(newPrice) + 0.99;
                newProduct.price = newPrice;

                //adding pictures
                foreach (var pic in newProdDataKVP.Where(x => x.Key.Contains("Picture"))) {
                    if (!string.IsNullOrEmpty(pic.Value) || !string.IsNullOrWhiteSpace(pic.Value)) {
                        newProduct.images.Add(pic.Value);
                    }
                }

                //adding vendor product type
                newProduct.productTypeVendor = newProductKVP["VendorType"];

                //adding product added timestamp
                newProduct.addedTimeStamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();

                //building Product description
                newProduct.body_html = BuildDescription(newProdDataKVP);

                ProductModule.AddProductToDB(newProduct);
                productAdded = true;
                return productAdded;
            }
        } 

        //method that gets product data KVP from XML node(from datasheet) 
        private Dictionary<string, string> GetProductDataKVP(XmlNode prodData) {
            Dictionary<string, string> prodDataKVP = new();

            int pictureCount = 1;

            foreach (XmlNode node in prodData.ChildNodes) {
                if (node.Name == "LongDesc" || node.Name == "ShortDesc") {
                    prodDataKVP.Add(node.Name, node.InnerText);
                } else {
                    var nodeAttributeVal = node.Attributes["descr"].Value;

                    if (nodeAttributeVal.Contains("Product Picture")) {
                        nodeAttributeVal = "Picture" + pictureCount++.ToString();
                    }

                    if (!prodDataKVP.ContainsKey(nodeAttributeVal)) {
                        prodDataKVP.Add(nodeAttributeVal, node.InnerText);
                    }
                }
            }
            return prodDataKVP;
        }

        //method that builds description for the product uisng datasheet KVP
        private string BuildDescription(Dictionary<string, string> prodDataKVP) {
            string description = "";

            string longDesc;
            bool longDescExists = prodDataKVP.TryGetValue("LongDesc", out longDesc);
            if (!longDescExists) { longDesc = ""; }
            if (!string.IsNullOrEmpty(longDesc) || !string.IsNullOrWhiteSpace(longDesc)) {
                description += longDesc + "<br><br>";
            }

            string marketingText;
            bool marketingTextExists = prodDataKVP.TryGetValue("Marketing Text", out marketingText);
            if (!marketingTextExists) { marketingText = ""; }
            if (!string.IsNullOrEmpty(marketingText) || !string.IsNullOrWhiteSpace(marketingText)) {
                description += marketingText + "<br><br>";
            }

            foreach (var skipableKey in descSkipableKeys) {
                if (prodDataKVP.ContainsKey(skipableKey)) {
                    prodDataKVP.Remove(skipableKey);
                }
            }

            StringBuilder sb = new StringBuilder();

            using (HTMLTable table = new HTMLTable(sb)) {
                foreach (var kvp in prodDataKVP) {
                    using (HTMLRow row = table.AddRow()) {
                        row.AddCell(kvp.Key);
                        row.AddCell(kvp.Value);
                    }
                }
            }

            string finishedTable = sb.ToString();
            description += finishedTable;

            description = description.Replace("'", "''");
            return description;
        }

    }
}
