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

        private Dictionary<string, string> APIParams = new Dictionary<string, string> {
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

        private static Dictionary<string, int> GetCategoriesDictionary() {

            //getting category KVP from database
            Dictionary<string, int> categoriesKVP = new();
            DataBaseInterface dataBaseInterface = new(DBsource);

            string cmdText = "select ID, ProductType from ProductTypes;";

            using (var QueryReader = dataBaseInterface.ExecQuery(cmdText)) {
                while (QueryReader.Read()) {
                    categoriesKVP.Add(QueryReader.GetString(1), QueryReader.GetInt32(0));
                }
            }
            return categoriesKVP;
        }

        //downloads Catalogue from TDB API
        private string GetAPICatalogue() {
            Dictionary<string, string> catalogueParams = APIParams;
            RESTClient restClient = new(BaseUrl);
            return restClient.ExecGet(CataloguePath, catalogueParams);
        }

        //get productID by SKU
        public static int GetID(string sku) {
            int id = new();
            string cmdText = $"SELECT ID from Products where SKU = '{sku}' limit 1;";

            DataBaseInterface dataBaseInterface = new(DBsource);
            using (var QueryReader = dataBaseInterface.ExecQuery(cmdText)) {
                while (QueryReader.Read()) {
                    id = QueryReader.GetInt32(0);
                }
            }
            return id;
        }

        public List<Product> getProductsFromDB() {
            List<Product> products = new();
            string cmdText = $"SELECT * from Products";
            Dictionary<string, int> categoriesKVP = GetCategoriesDictionary();
            Dictionary<int, string> categoriesKVPR = categoriesKVP.ToDictionary(x => x.Value, x => x.Key);

            //getting main product info
            DataBaseInterface dataBaseInterface = new(DBsource);
            using (var QueryReader = dataBaseInterface.ExecQuery(cmdText)) {
                while (QueryReader.Read()) {
                    Product NewProduct = new();
                    NewProduct.BDID = QueryReader.GetInt32(0);
                    NewProduct.title = QueryReader.GetString(1);
                    NewProduct.body_html = QueryReader.GetString(2);
                    NewProduct.vendor = QueryReader.GetString(3);
                    NewProduct.product_type = categoriesKVPR[QueryReader.GetInt32(4)];
                    NewProduct.price = double.Parse(QueryReader.GetString(5));
                    NewProduct.sku = QueryReader.GetString(6);
                    NewProduct.stock = QueryReader.GetInt32(7);
                    NewProduct.barcode = QueryReader.GetString(8);
                    NewProduct.vendor_price = double.Parse(QueryReader.GetString(9));
                    NewProduct.weight = double.Parse(QueryReader.GetString(10));
                    NewProduct.height = QueryReader.GetInt32(11);
                    NewProduct.lenght = QueryReader.GetInt32(12);
                    NewProduct.width = QueryReader.GetInt32(13);

                    products.Add(NewProduct);
                }
            }

            //getting images faster
            cmdText = $"select * from Images";
            dataBaseInterface = new(DBsource);
            using (var QueryReader = dataBaseInterface.ExecQuery(cmdText)) {
                while (QueryReader.Read()) {
                    int productID = QueryReader.GetInt32(1);
                    string imageUrl = QueryReader.GetString(2);

                    products.Find(x => x.BDID == productID).images.Add(imageUrl);
                }
            }

            //getting tags faster
            cmdText = $"select * from Tags";
            dataBaseInterface = new(DBsource);
            using (var QueryReader = dataBaseInterface.ExecQuery(cmdText)) {
                while (QueryReader.Read()) {
                    int productID = QueryReader.GetInt32(1);
                    string tag = QueryReader.GetString(2);

                    products.Find(x => x.BDID == productID).tags.Add(tag);
                }
            }

            return products;
        }

        ////must be used one time
        //public void InsertProductsToDB() {

        //    List<Product> products = CreateProductList();
        //    Dictionary<string, int> categoriesKVP = GetCategoriesDictionary();

        //    for (int i = 0; i < products.Count; i++) {
        //        Product product = products[i];

        //        //inserting product 
        //        string priceStr = product.price.ToString();
        //        string stockStr = product.stock.ToString();
        //        string vendor_priceStr = product.vendor_price.ToString();
        //        string weightStr = product.weight.ToString();
        //        string heightStr = product.height.ToString();
        //        string lenghtStr = product.lenght.ToString();
        //        string widthStr = product.width.ToString();

        //        int productTypeID = categoriesKVP[product.product_type];

        //        string insertCmd = $"insert into Products(Title, Body, Vendor, ProductType, Price, SKU, Stock, Barcode, PriceVendor, Weight, Height, Lenght, Width) " +
        //            $"VALUES('{product.title}', '{product.body_html}', '{product.vendor}', '{productTypeID}', '{priceStr}', '{product.sku}', '{stockStr}', '{product.barcode}', '{vendor_priceStr}', '{weightStr}', '{heightStr}', '{lenghtStr}', '{widthStr}');";


        //        DataBaseInterface dataBaseInterface = new(DBsource);
        //        dataBaseInterface.ExecNonQuery(insertCmd);
        //        int prodDBID = GetID(product.sku);

        //        //inserting product images
        //        foreach (string img in product.images) {
        //            string insertImgCmd = $"insert into Images(ProductID, ImgUrl) VALUES('{prodDBID}', '{img}');";
        //            dataBaseInterface.ExecNonQuery(insertImgCmd);
        //        }

        //        //todo:inserting product tags
        //        foreach (string tag in product.tags) {
        //            string insertImgCmd = $"insert into Tags(ProductID, Tag) VALUES('{prodDBID}', '{tag}');";
        //            dataBaseInterface.ExecNonQuery(insertImgCmd);
        //        }

        //        Debug.WriteLine($"{i}/{products.Count}");
        //    }
        //}

        ////must be one time use
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

        //    return products;
        //}

        ////must be one time use 
        //public void InsertCategoriesToDB() {

        //    //getting category KVP from json
        //    Dictionary<string, string> categoriesKVP;
        //    using (StreamReader r = new StreamReader(TDBCategoriesJson)) {
        //        string json = r.ReadToEnd();
        //        categoriesKVP = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        //    }

        //    //inserting pairs into database
        //    DataBaseInterface dataBaseInterface = new(DBsource);
        //    foreach (var pair in categoriesKVP) {
        //        dataBaseInterface.ExecNonQuery($"insert into ProductTypes(ProductType, ProductTypeVendor) VALUES('{pair.Value}', '{pair.Key}');");

        //    }
        //}

    }
}
