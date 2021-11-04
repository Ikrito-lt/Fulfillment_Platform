using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Linq;

namespace Ikrito_Fulfillment_Platform.Modules {
    class TDBModule {

        private Dictionary<string, string> APIParams = new Dictionary<string, string> {
            ["orgnum"] = "268230",
            ["username"] = "PREKES",
            ["pwd"] = "Welcome.123",
            ["ean"] = "y"
        };

        private static string BaseUrl = "http://tdonline.tdbaltic.net/pls/PROD/";
        private static string CataloguePath = "ixml.ProdCatExt";
        private static string DataSheetsPath = "ixml.DSheets";
        private static string DBTablePrefix = "TDB_";
        

        //private readonly string TDBDesc_location = @"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Files\TDB\TDB_cat.xml";
        //private readonly string TDBCat_location = @"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Files\TDB\TDB_cat2.xml";
        //private readonly string TDBCategoriesJson = @"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Files\TDB\TDBCategories.json";

        //downloads Catalogue from TDB API
        private string GetAPICatalogue() {
            Dictionary<string, string> catalogueParams = APIParams;
            RESTClient restClient = new(BaseUrl);
            return restClient.ExecGetParams(CataloguePath, catalogueParams);
        }

        public void updateTDBProducts(object sender = null, DoWorkEventArgs e = null) {
            string strCatalogue = GetAPICatalogue();
            XmlDocument categoryXML = new();
            categoryXML.LoadXml(strCatalogue);

            List<Dictionary<string, string>> changeList = new();

            var catalogueProducts = categoryXML.FirstChild.ChildNodes;
            foreach (XmlNode prodXML in catalogueProducts) {
                Dictionary<string, string> productInfo = new();

                XmlNode skuNode = prodXML.SelectSingleNode("TDPartNbr");
                XmlNode priceNode = prodXML.SelectSingleNode("Price");
                XmlNode stockNode = prodXML.SelectSingleNode("Stock");
                XmlNode barcodeNode = prodXML.SelectSingleNode("Ean");

                productInfo.Add("SKU", "TDB-" + skuNode.InnerText);
                productInfo.Add("Price", priceNode.InnerText);
                productInfo.Add("Stock", stockNode.InnerText);
                productInfo.Add("Barcode", barcodeNode.InnerText);

                changeList.Add(productInfo);
            }

            //getting sku product from sku table Product table
            DataBaseInterface db = new();
            var productTable = db.Table("Products").Get();
            var products = productTable.Values.ToList();
            products.RemoveAll(x => !x["SKU"].StartsWith("TDB-"));

            foreach (var product in products) {
                string sku = product["SKU"];
                var productChanges = changeList.Find(x => x["SKU"] == sku);

                db = new();
                var whereQuery = new Dictionary<string, Dictionary<string, string>> {
                    ["SKU"] = new Dictionary<string, string> {
                        ["="] = sku
                    }
                };
                var oldProductDB = db.Table("TDB_Products").Where(whereQuery).Get()[0];

                //todo: figure out this logic
                if (oldProductDB["PriceVendor"] != productChanges["Price"]) { 
                
                }
                if (oldProductDB["PriceVendor"] != productChanges["Price"]) {

                }

            }


        }

        public void updateTDBProductsComplete(object sender, RunWorkerCompletedEventArgs e) {

        }

        //private List<Product> CreateProductList() {

        //    XmlDocument TDB_desc = new();
        //    XmlDocument TDB_cat = new();
        //    TDB_desc.Load(TDBDesc_location);
        //    TDB_cat.Load(TDBCat_location);

        //    var TDB_desc_products = TDB_desc.ChildNodes.Item(0).ChildNodes;
        //    var TDB_cat_products = TDB_cat.ChildNodes.Item(0).ChildNodes;

        //    List<Product> products = new();

        //    for (int i = 0; i < TDB_cat_products.Count; i++) {
        //        XmlNode CProd = TDB_cat_products.Item(i);
        //        string CProdID = CProd.SelectSingleNode("productID").InnerText;

        //        XmlNode DProd = TDB_desc_products.Item(i);
        //        string DProdID = DProd.SelectSingleNode("productID").InnerText;

        //        if (CProdID != DProdID) {
        //            foreach (XmlNode prod in TDB_desc_products) {
        //                string id = prod.SelectSingleNode("productID").InnerText;
        //                if (id == CProdID) {
        //                    DProd = prod;
        //                    DProdID = id;
        //                    break;
        //                }
        //            }
        //        }

        //        Product newProduct = new();
        //        newProduct.title = CProd.SelectSingleNode("product_title").InnerText;
        //        newProduct.body_html = DProd.SelectSingleNode("new_desc").InnerText;
        //        newProduct.vendor = CProd.SelectSingleNode("vendor").InnerText;
        //        newProduct.product_type = CProd.SelectSingleNode("product_type").InnerText;
        //        newProduct.price = double.Parse(DProd.SelectSingleNode("kaina").InnerText, System.Globalization.CultureInfo.InvariantCulture);
        //        newProduct.sku = CProdID;
        //        newProduct.barcode = CProd.SelectSingleNode("barcode").InnerText;
        //        newProduct.vendor_price = double.Parse(DProd.SelectSingleNode("TDB_price").InnerText, System.Globalization.CultureInfo.InvariantCulture);

        //        //handling stock
        //        string stockStr = DProd.SelectSingleNode("stock").InnerText;
        //        if (stockStr != "" && stockStr != null) {
        //            newProduct.stock = int.Parse(DProd.SelectSingleNode("stock").InnerText, System.Globalization.CultureInfo.InvariantCulture);
        //        } else {
        //            newProduct.stock = 0;
        //        }

        //        //getting weight
        //        string weightStr = CProd.SelectSingleNode("weight").InnerText;
        //        if (weightStr.EndsWith(" Kg")) {
        //            weightStr = weightStr.Substring(0, weightStr.Length - 3);
        //            newProduct.weight = double.Parse(weightStr, System.Globalization.CultureInfo.InvariantCulture);
        //        } else {
        //            newProduct.weight = 0;
        //        }

        //        //adding images 
        //        string img1 = CProd.SelectSingleNode("product_images").InnerText;
        //        if (img1 != "") newProduct.images.Add(img1);
        //        string img2 = CProd.SelectSingleNode("product_images2").InnerText;
        //        if (img2 != "") newProduct.images.Add(img2);
        //        string img3 = CProd.SelectSingleNode("product_images3").InnerText;
        //        if (img3 != "") newProduct.images.Add(img3);
        //        string img4 = CProd.SelectSingleNode("product_images4").InnerText;
        //        if (img4 != "") newProduct.images.Add(img4);

        //        products.Add(newProduct);
        //    }

    }
}
