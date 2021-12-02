﻿using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;

namespace Ikrito_Fulfillment_Platform.Modules {
    class TDBModule {

        private static readonly Dictionary<string, string> _APIParams = Globals._TDBAPIParams;

        private readonly List<string> descSkipableKeys = new(){
                "Manufacturer Logo",
                "Picture1",
                "Picture2",
                "Picture3",
                "Picture4",
                "LongDesc",
                "ShortDesc",
                "Marketing Text"
        };

        private const string _BaseUrl = "http://tdonline.tdbaltic.net/pls/PROD/";
        private const string _CataloguePath = "ixml.ProdCatExt";
        private const string _DataSheetsPath = "ixml.DSheets";

        private readonly Lazy<XmlDocument> _LazyDataSheetXML = new(() => GetTDBDataSheets());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private XmlDocument _DataSheetsXML => _LazyDataSheetXML.Value;

        private readonly Lazy<XmlDocument> _LazyCategoryXML = new(() => GetTDBCatalogue());
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
                    try {
                        ProductModule.ChangeProductStatus(sku, ProductStatus.NeedsArchiving);
                    }
                    catch (Exception ex) {
                        MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    continue;
                } else {

                    //get product from db 
                    var whereQuery = new Dictionary<string, Dictionary<string, string>> {
                        ["SKU"] = new Dictionary<string, string> {
                            ["="] = sku
                        }
                    };
                    var result = db.Table("TDB_Products").Where(whereQuery).Get();
                    var oldProductDB = result[0];

                    //if data is the same dont change shit 
                    if (oldProductDB["Stock"] == productChanges["Stock"] && oldProductDB["PriceVendor"] == productChanges["PriceVendor"]) {
                        pendingChanges.RemoveAll(x => x["SKU"] == sku);
                        continue;
                    } else {
                        
                        appliedChanges.Add(sku, new Dictionary<string, string>() { 
                            ["Stock"] = "",
                            ["PriceVendor"] = "",
                            ["Price"] = "",
                        });

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
                            appliedChanges[sku]["Stock"] = $"{oldProductDB["Stock"]} -> {productChanges["Stock"]}";
                        }

                        //updating price
                        if (oldProductDB["PriceVendor"] != productChanges["PriceVendor"]) {
                            double newSalePrice = PriceGenModule.GenNewPrice(double.Parse(productChanges["PriceVendor"]));

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
                            appliedChanges[sku]["PriceVendor"] = $"{oldProductDB["PriceVendor"]} -> {productChanges["PriceVendor"]}";
                            appliedChanges[sku]["Price"] = $"{oldProductDB["Price"]} -> {newSalePrice}";
                        }

                        try {
                            ProductModule.ChangeProductStatus(sku, ProductStatus.WaitingShopSync);
                        }
                        catch (Exception ex) {
                            MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
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
            changes.Add("UpdatedProducts", appliedChanges);
            changes.Add("InvalidProducts", pendingChanges);
            changes.Add("NewProducts", newProducts);
            if (e != null) {
                e.Result = changes;
            }
        }

        // adds new TDB product to database
        private bool addNewTDBProduct(Dictionary<string, string> newProductKVP) {
            bool productAdded = false;
            string newProdSKU = newProductKVP["SKU"];
            string newProdTDBSKU = newProdSKU[(newProdSKU.IndexOf('-') + 1)..];

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
                bool grossExists = newProdDataKVP.TryGetValue("Gross Weight", out string grossWeightStr);
                string netWeightStr = "0";
                bool netExists = newProdDataKVP.TryGetValue("Net Weight", out grossWeightStr);

                if (!grossExists) { grossWeightStr = "0"; }
                if (!netExists) { netWeightStr = "0"; }

                grossWeightStr = grossWeightStr.Split(" ")[0];
                netWeightStr = netWeightStr.Split(" ")[0];


                bool grossWeightConvSucceded = double.TryParse(grossWeightStr, out double grossWeight);
                if (!grossWeightConvSucceded) {grossWeight = .0;}
                bool netWeightConvSucceded = double.TryParse(netWeightStr, out double netWeight);
                if (!netWeightConvSucceded) { netWeight = .0; }
                newProduct.weight = Math.Max(grossWeight, netWeight);

                //getting height
                bool heightExists = newProdDataKVP.TryGetValue("Height", out string heightStr);
                if (!heightExists) { heightStr = "0"; }

                heightStr = heightStr.Split(" ")[0];
                bool heightConvSucceded = int.TryParse(heightStr, out int heightInt);
                if (heightConvSucceded) {
                    newProduct.height = heightInt;
                } else {
                    newProduct.height = 0;
                }

                //getting lenght
                bool lenghtExists = newProdDataKVP.TryGetValue("Lenght", out string lenghtStr);
                if (!lenghtExists) { lenghtStr = "0"; }

                lenghtStr = lenghtStr.Split(" ")[0];
                bool lenghtConvSucceded = int.TryParse(lenghtStr, out int lenghtInt);
                if (lenghtConvSucceded) {
                    newProduct.lenght = lenghtInt;
                } else {
                    newProduct.lenght = 0;
                }

                //getting width
                bool widthExists = newProdDataKVP.TryGetValue("Width", out string widthStr);
                if (!widthExists) { widthStr = "0"; }

                widthStr = widthStr.Split(" ")[0];
                bool widthConvSucceded = int.TryParse(widthStr, out int widthInt);
                if (widthConvSucceded) {
                    newProduct.width = widthInt;
                } else {
                    newProduct.width = 0;
                }

                //calvulating newProduct price with 15-30% margin choosen on random
                double NewSalePrice = PriceGenModule.GenNewPrice(newProduct.vendor_price);
                newProduct.price = NewSalePrice;

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
        private static Dictionary<string, string> GetProductDataKVP(XmlNode prodData) {
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

            bool longDescExists = prodDataKVP.TryGetValue("LongDesc", out string longDesc);
            if (!longDescExists) { longDesc = ""; }
            if (!string.IsNullOrEmpty(longDesc) || !string.IsNullOrWhiteSpace(longDesc)) {
                description += longDesc + "<br><br>";
            }

            bool marketingTextExists = prodDataKVP.TryGetValue("Marketing Text", out string marketingText);
            if (!marketingTextExists) { marketingText = ""; }
            if (!string.IsNullOrEmpty(marketingText) || !string.IsNullOrWhiteSpace(marketingText)) {
                description += marketingText + "<br><br>";
            }

            foreach (var skipableKey in descSkipableKeys) {
                if (prodDataKVP.ContainsKey(skipableKey)) {
                    prodDataKVP.Remove(skipableKey);
                }
            }

            StringBuilder sb = new();

            using (HTMLTable table = new(sb)) {
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