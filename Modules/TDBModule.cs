﻿using Ikrito_Fulfillment_Platform.Models;
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

        private Dictionary<int, string> GetCategoriesDictionary() {

            //getting category KVP from database
            Dictionary<int, string> categoriesKVP = new();
            DataBaseInterface dataBaseInterface = new(DBsource);

            string cmdText = "select ID, ProductType from ProductTypes;";

            using (var QueryReader = dataBaseInterface.ExecQuery(cmdText)) {
                while (QueryReader.Read()) {
                    categoriesKVP.Add(QueryReader.GetInt32(0), QueryReader.GetString(1));
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

        ////must be one time use 
        //private void InsertCategoriesToDB() {

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

        //must be used one time
        public void InsertProductsToDB() {

            List<Product> products = CreateProductList();

            foreach (Product product in products) { 
            
                
            }
            



        }

        //must be one time use
        private List<Product> CreateProductList() {

            XmlDocument TDB_desc = new();
            XmlDocument TDB_cat = new();
            TDB_desc.Load(TDBDesc_location);
            TDB_cat.Load(TDBCat_location);

            var TDB_desc_products = TDB_desc.ChildNodes.Item(0).ChildNodes;
            var TDB_cat_products = TDB_cat.ChildNodes.Item(0).ChildNodes;

            List<Product> products = new();

            for (int i = 0; i < TDB_cat_products.Count; i++) {
                XmlNode CProd = TDB_cat_products.Item(i);
                string CProdID = CProd.SelectSingleNode("productID").InnerText;

                XmlNode DProd = TDB_desc_products.Item(i);
                string DProdID = DProd.SelectSingleNode("productID").InnerText;

                if (CProdID != DProdID) {
                    foreach (XmlNode prod in TDB_desc_products) {
                        string id = prod.SelectSingleNode("productID").InnerText;
                        if (id == CProdID) {
                            DProd = prod;
                            DProdID = id;
                            break;
                        }
                    }
                }

                Product newProduct = new();
                newProduct.title = CProd.SelectSingleNode("product_title").InnerText;
                newProduct.body_html = DProd.SelectSingleNode("new_desc").InnerText;
                newProduct.vendor = CProd.SelectSingleNode("vendor").InnerText;
                newProduct.product_type = CProd.SelectSingleNode("product_type").InnerText;
                newProduct.price = double.Parse(DProd.SelectSingleNode("kaina").InnerText, System.Globalization.CultureInfo.InvariantCulture);
                newProduct.sku = CProdID;
                newProduct.barcode = CProd.SelectSingleNode("barcode").InnerText;
                newProduct.vendor_price = double.Parse(DProd.SelectSingleNode("TDB_price").InnerText, System.Globalization.CultureInfo.InvariantCulture);

                //handling stock
                string stockStr = DProd.SelectSingleNode("stock").InnerText;
                if (stockStr != "" && stockStr != null) {
                    newProduct.stock = int.Parse(DProd.SelectSingleNode("stock").InnerText, System.Globalization.CultureInfo.InvariantCulture);
                } else {
                    newProduct.stock = 0;
                }

                //getting weight
                string weightStr = CProd.SelectSingleNode("weight").InnerText;
                if (weightStr.EndsWith(" Kg")) {
                    weightStr = weightStr.Substring(0, weightStr.Length - 3);
                    newProduct.weight = double.Parse(weightStr, System.Globalization.CultureInfo.InvariantCulture);
                } else {
                    newProduct.weight = 0;
                }

                //adding images 
                string img1 = CProd.SelectSingleNode("product_images").InnerText;
                if (img1 != "") newProduct.images.Add(img1);
                string img2 = CProd.SelectSingleNode("product_images2").InnerText;
                if (img2 != "") newProduct.images.Add(img2);
                string img3 = CProd.SelectSingleNode("product_images3").InnerText;
                if (img3 != "") newProduct.images.Add(img3);
                string img4 = CProd.SelectSingleNode("product_images4").InnerText;
                if (img4 != "") newProduct.images.Add(img4);

                products.Add(newProduct);
            }

            return products;
        }

    }
}
