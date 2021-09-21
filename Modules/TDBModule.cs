using Ikrito_Fulfillment_Platform.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ikrito_Fulfillment_Platform.Modules {
    class TDBModule {

        private  Dictionary<string, string> APIParams = new Dictionary<string, string> {
            ["orgnum"] = "268230",
            ["username"] = "PREKES",
            ["pwd"] = "Welcome.123",
            ["ean"] = "y"
        };

        private const string BaseUrl = "http://tdonline.tdbaltic.net/pls/PROD/";
        private const string CataloguePath = "ixml.ProdCatExt";
        private const string DataSheetsPath = "ixml.DSheets";

        public static readonly string DBsource = @"Data Source=C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\TDB.db;Version=3;";

        private readonly string TDBDesc_location = @"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Files\TDB\TDB_cat.xml";
        private readonly string TDBCat_location = @"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Files\TDB\TDB_cat2.xml";
        private readonly string TDBCategoriesJson = @"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Files\TDB\TDBCategories.json";


        //must be one time use 
        public void insertCategoriesToDB() {

            //getting category KVP from json
            Dictionary<string, string> categoriesKVP;
            using (StreamReader r = new StreamReader(TDBCategoriesJson)) {
                string json = r.ReadToEnd();
                categoriesKVP = JsonConvert.DeserializeObject<Dictionary<string,string>>(json);
            }

            //inserting pairs into database
            DataBaseInterface dataBaseInterface = new(DBsource);
            foreach (var pair in categoriesKVP) {
                dataBaseInterface.ExecNonQuery($"insert into ProductTypes(ProductType, ProductTypeVendor) VALUES('{pair.Value}', '{pair.Key}');");
                
            }
        }




        //List<Product> productsTDB;

        //XmlDocument TDB_desc = new();
        //XmlDocument TDB_cat = new();

        public TDBModule() {
            //TDB_desc.Load(TDBDesc_location);
            //TDB_cat.Load(TDBCat_location);
        }

        //public List<Product> CreateProductList() {
        //    var TDB_desc_products = TDB_desc.ChildNodes.Item(0).ChildNodes;
        //    var TDB_cat_products = TDB_cat.ChildNodes.Item(0).ChildNodes;

        //    List<Product> products = new();

        //    for (int i = 0; i < 1; i++) {
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
        //        newProduct.stock = int.Parse(DProd.SelectSingleNode("stock").InnerText, System.Globalization.CultureInfo.InvariantCulture);
        //        newProduct.barcode = CProd.SelectSingleNode("barcode").InnerText;
        //        newProduct.vendor_price = double.Parse(DProd.SelectSingleNode("TDB_price").InnerText, System.Globalization.CultureInfo.InvariantCulture);

        //        //getting weight
        //        string weightStr = CProd.SelectSingleNode("weight").InnerText;
        //        weightStr = weightStr.Substring(0, weightStr.Length - 3);
        //        newProduct.weight = double.Parse(weightStr, System.Globalization.CultureInfo.InvariantCulture);

        //        //adding images 
        //        string img1 = CProd.SelectSingleNode("product_images").InnerText;
        //        if (img1 != "") newProduct.images.Add(img1);
        //        string img2 = CProd.SelectSingleNode("product_images2").InnerText;
        //        if (img2 != "") newProduct.images.Add(img2);
        //        string img3 = CProd.SelectSingleNode("product_images3").InnerText;
        //        if (img3 != "") newProduct.images.Add(img3);
        //        string img4 = CProd.SelectSingleNode("product_images4").InnerText;
        //        if (img4 != "") newProduct.images.Add(img4);


        //    }

        //    return products;
        //}

        


        //public void PushToShop() { 
        
        //}

        //public void PushToDB() {

        //    var TDB_desc_products = TDB_desc.ChildNodes.Item(0).ChildNodes;
        //    var TDB_cat_products = TDB_cat.ChildNodes.Item(0).ChildNodes;

        //    Debug.WriteLine($"desc count: {TDB_desc_products.Count}");
        //    Debug.WriteLine($"desc count: {TDB_cat_products.Count}");


        //}

    }
}
