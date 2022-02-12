using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using static Ikrito_Fulfillment_Platform.Models.FullProduct;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier.TDBaltic {
    static class TDBModule {

        private static readonly Dictionary<string, string> _APIParams = Globals._TDBAPIParams;

        private const string _BaseUrl = "http://tdonline.tdbaltic.net/pls/PROD/";
        private const string _CataloguePath = "ixml.ProdCatExt";
        private const string _DataSheetsPath = "ixml.DSheets";

        private static readonly List<string> descSkipableKeys = new(){
                "Manufacturer Logo",
                "Manufacturer Logo URL",
                "Picture1",
                "Picture2",
                "Picture3",
                "Picture4",
                "LongDesc",
                "ShortDesc"
        };

        private static readonly List<string> SKUBlackList = new()
        {
            "TDB-Network-M2".ToLower()
        };

        private static readonly Lazy<XmlDocument> _LazyDataSheetXML = new(() => GetTDBDataSheets());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static XmlDocument _DataSheetsXML => _LazyDataSheetXML.Value;

        private static readonly Lazy<XmlDocument> _LazyCategoryXML = new(() => GetTDBCatalogue());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static XmlDocument _CategoryXML => _LazyCategoryXML.Value;

        private static readonly Lazy<List<FullProduct>> _LazyProductList = new(() => BuildProductList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<FullProduct> ProductList => _LazyProductList.Value;


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
        // Section for automatically updating and adding products to database
        //

        //// Updates product and then sends products that dont exist to addNewProduct method
        //public static void UpdateTDBProducts(object sender = null, DoWorkEventArgs e = null) {
        //    List<Product> DBProducts = ProductModule.GetVendorProducts("TDB");
        //    List<Product> APIProducts = BuildProductList();

        //    List<Product> ArchiveProducts = DBProducts.Where(p1 => APIProducts.All(p2 => p2.sku != p1.sku)).ToList();
        //    List<Product> NewProducts = APIProducts.Where(p1 => DBProducts.All(p2 => p2.sku != p1.sku)).ToList();
        //    List<Product> UpdateProducts = APIProducts.Where(p1 => NewProducts.All(p2 => p2.sku != p1.sku)).ToList();

        //    //remove dublicate skus from newProd list
        //    var a = NewProducts.GroupBy(x => x.sku.ToLower()).Where(x => x.LongCount() > 1).ToList();
        //    a.ForEach(x => NewProducts.RemoveAll(y => y.sku.ToLower() == x.Key));

        //    Dictionary<string, Dictionary<string, string>> appliedChanges = new();          //for updates
        //    List<Dictionary<string, string>> newChanges = new();                            //for new products
        //    List<Dictionary<string, string>> archivedChanges = new();                       //for archived Products                     

        //    //archiving products
        //    foreach (Product archiveProduct in ArchiveProducts) {
        //        try {
        //            ProductModule.ChangeProductStatus(archiveProduct.sku, ProductStatus.NeedsArchiving);

        //            Dictionary<string, string> archiveChange = new();
        //            archiveChange.Add("SKU", archiveProduct.sku);
        //            archiveChange.Add("PriceVendor", archiveProduct.productVariants.First().vendor_price.ToString());
        //            archiveChange.Add("Price", archiveProduct.productVariants.First().price.ToString());
        //            archiveChange.Add("Stock", archiveProduct.productVariants.First().stock.ToString());
        //            archiveChange.Add("Barcode", archiveProduct.productVariants.First().barcode);
        //            archiveChange.Add("Vendor", archiveProduct.vendor);
        //            archiveChange.Add("VendorType", archiveProduct.productTypeVendor);
        //            archivedChanges.Add(archiveChange);
        //        }
        //        catch (Exception ex) {
        //            MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", 
        //                "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }

        //    // adding new Products
        //    foreach (Product newProduct in NewProducts) {

        //        if (SKUBlackList.Contains(newProduct.sku.ToLower())) continue;
        //        ProductModule.AddProductToDB(newProduct);

        //        Dictionary<string, string> newChange = new();
        //        newChange.Add("SKU", newProduct.sku);
        //        newChange.Add("PriceVendor", newProduct.productVariants.First().vendor_price.ToString());
        //        newChange.Add("Price", newProduct.productVariants.First().price.ToString());
        //        newChange.Add("Stock", newProduct.productVariants.First().stock.ToString());
        //        newChange.Add("Barcode", newProduct.productVariants.First().barcode);
        //        newChange.Add("Vendor", newProduct.vendor);
        //        newChange.Add("VendorType", newProduct.productTypeVendor);
        //        newChanges.Add(newChange);
        //    }

        //    DataBaseInterface db = new();

        //    //updating products
        //    foreach (Product updateProduct in UpdateProducts) {
        //        Product oldProduct = DBProducts.Find(x => x.sku == updateProduct.sku);

        //        //if no changes skip
        //        if (updateProduct.productVariants.First().stock == oldProduct.productVariants.First().stock && updateProduct.productVariants.First().vendor_price == oldProduct.productVariants.First().vendor_price) {
        //            continue;
        //        } else {

        //            appliedChanges.Add(oldProduct.sku, new Dictionary<string, string>() {
        //                ["Stock"] = "",
        //                ["PriceVendor"] = "",
        //                ["Price"] = "",
        //            });

        //            //update stock
        //            if (updateProduct.productVariants.First().stock != oldProduct.productVariants.First().stock) {
        //                var stockUpdateData = new Dictionary<string, string> {
        //                    ["Stock"] = updateProduct.productVariants.First().stock.ToString()
        //                };
        //                var stockWhereUpdate = new Dictionary<string, Dictionary<string, string>> {
        //                    ["SKU"] = new Dictionary<string, string> {
        //                        ["="] = oldProduct.sku
        //                    }
        //                };
        //                db.Table("_TDB_Variants").Where(stockWhereUpdate).Update(stockUpdateData);

        //                //adding change to applied change list
        //                appliedChanges[oldProduct.sku]["Stock"] = $"{oldProduct.productVariants.First().stock} -> {updateProduct.productVariants.First().stock}";
        //            }

        //            //update price
        //            if (updateProduct.productVariants.First().vendor_price != oldProduct.productVariants.First().vendor_price) {
        //                //updating price value
        //                var priceUpdateData = new Dictionary<string, string> {
        //                    ["PriceVendor"] = updateProduct.productVariants.First().vendor_price.ToString(),
        //                    ["Price"] = updateProduct.productVariants.First().price.ToString()
        //                };
        //                var priceWhereUpdate = new Dictionary<string, Dictionary<string, string>> {
        //                    ["SKU"] = new Dictionary<string, string> {
        //                        ["="] = oldProduct.sku
        //                    }
        //                };
        //                db.Table("_TDB_Products").Where(priceWhereUpdate).Update(priceUpdateData);

        //                //adding change to applied change list
        //                appliedChanges[oldProduct.sku]["PriceVendor"] = $"{oldProduct.productVariants.First().vendor_price} -> {updateProduct.productVariants.First().vendor_price}";
        //                appliedChanges[oldProduct.sku]["Price"] = $"{oldProduct.productVariants.First().price} -> {updateProduct.productVariants.First().price}";
        //            }

        //            //updating product status
        //            try {
        //                ProductModule.ChangeProductStatus(oldProduct.sku, ProductStatus.WaitingShopSync);
        //            }
        //            catch (Exception ex) {
        //                MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }
        //    }

        //    //pass applied changes and pending changes to update on complete method
        //    Dictionary<string, object> changes = new();
        //    changes.Add("UpdatedProducts", appliedChanges);
        //    changes.Add("ArchivedProducts", archivedChanges);
        //    changes.Add("NewProducts", newChanges);
        //    if (e != null) {
        //        e.Result = changes;
        //    }
        //}

        //method that builds product list from TDB API data
        private static List<FullProduct> BuildProductList() {
            List<FullProduct> NewProducts = new();

            var catalogueProducts = _CategoryXML.FirstChild.ChildNodes;
            foreach (XmlNode prodXML in catalogueProducts) {

                XmlNode skuNode = prodXML.SelectSingleNode("TDPartNbr");
                XmlNode priceVendorNode = prodXML.SelectSingleNode("Price");
                XmlNode stockNode = prodXML.SelectSingleNode("Stock");
                XmlNode barcodeNode = prodXML.SelectSingleNode("Ean");
                XmlNode vendorNode = prodXML.SelectSingleNode("Manuf");
                XmlNode vendorTypeNode = prodXML.SelectSingleNode("SubClassCode");

                string TDBSKU = skuNode.InnerText;
                string NewSKU = "TDB-" + TDBSKU;

                var newProdDataXML = _DataSheetsXML.SelectSingleNode(@$"/Datasheets/Datasheet[@TDPartNbr='{TDBSKU}']");
                if (newProdDataXML == null) {
                    continue;
                } else {
                    Dictionary<string, string> newProdDataKVP = GetProductDataKVP(newProdDataXML);

                    //since TDB has only one variant for one product we add only one variant

                    //init new product object
                    FullProduct newProduct = new();
                    ProductVariant newVariant = new(); 

                    string title = newProdDataKVP["ShortDesc"];
                    title = SQLUtil.SQLSafeString(title);

                    newProduct.title = title;
                    newProduct.vendor = vendorNode.InnerText;
                    newProduct.productTypeID = 1.ToString();
                    newProduct.sku = NewSKU;
                    newVariant.stock = int.Parse(stockNode.InnerText);
                    newVariant.barcode = barcodeNode.InnerText;
                    newVariant.vendor_price = double.Parse(priceVendorNode.InnerText);
                    newVariant.PermPrice = false;

                    //getting weight
                    bool grossExists = newProdDataKVP.TryGetValue("Gross Weight", out string grossWeightStr);
                    string netWeightStr = "0";
                    bool netExists = newProdDataKVP.TryGetValue("Net Weight", out grossWeightStr);

                    if (!grossExists) { grossWeightStr = "0"; }
                    if (!netExists) { netWeightStr = "0"; }

                    grossWeightStr = grossWeightStr.Split(" ")[0];
                    netWeightStr = netWeightStr.Split(" ")[0];

                    bool grossWeightConvSucceded = double.TryParse(grossWeightStr, out double grossWeight);
                    if (!grossWeightConvSucceded) { grossWeight = .0; }
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

                    //calculating newProduct price
                    double NewSalePrice = PriceGenModule.GenNewPrice(newVariant.vendor_price);
                    newVariant.price = NewSalePrice;

                    //adding pictures
                    foreach (var pic in newProdDataKVP.Where(x => x.Key.Contains("Picture"))) {
                        if (!string.IsNullOrEmpty(pic.Value) || !string.IsNullOrWhiteSpace(pic.Value)) {
                            newProduct.images.Add(pic.Value);
                        }
                    }

                    //adding vendor product type
                    newProduct.productTypeVendor = vendorTypeNode.InnerText;

                    //adding product added timestamp
                    newProduct.addedTimeStamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();

                    //building Product description
                    newProduct.body_html = BuildDescription(newProdDataKVP);

                    if (newProduct.height == 0) newProduct.height = 1;
                    if (newProduct.width == 0) newProduct.width = 1;
                    if (newProduct.lenght == 0) newProduct.lenght = 1;

                    //adding variant and product attributes to the product model
                    foreach (KeyValuePair<string,string> pDataKVP in newProdDataKVP) {
                        newProduct.productAttributtes.Add(pDataKVP.Key, SQLUtil.SQLSafeString(pDataKVP.Value));
                    }

                    newProduct.productVariants.Add(newVariant);
                    NewProducts.Add(newProduct);
                }
            }

            //removing blackilisted products from the list
            NewProducts.RemoveAll(x => SKUBlackList.Contains(x.sku.ToLower()));

            return NewProducts;
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
        private static string BuildDescription(Dictionary<string, string> prodDataKVP) {
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

            description = SQLUtil.SQLSafeString(description);
            return description;
        }
    }
}
