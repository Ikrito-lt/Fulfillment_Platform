using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules {
    class ProductModule {

        public static Dictionary<string, string> GetCategoriesDictionary() {

            //getting category KVP from database
            Dictionary<string, string> categoriesKVP = new();
            DataBaseInterface db = new();

            var result = db.Table("ProductTypes").Get("ID, ProductType");

            foreach (var cat in result.Values) {

                var id = cat["ID"];
                var type = cat["ProductType"];

                categoriesKVP.Add(id, type);
            }

            return categoriesKVP;
        }

        public static Product GetProduct(string sku) {
            Product prod = new();
            var categoriesKVP = GetCategoriesDictionary();

            string tablePrefix = sku.GetUntilOrEmpty();

            DataBaseInterface db = new();
            Dictionary<string, Dictionary<string, string>> whereCond;
            whereCond = new Dictionary<string, Dictionary<string, string>> {
                ["SKU"] = new Dictionary<string, string> {
                    ["="] = sku
                }
            };

            var result = db.Table(tablePrefix+"_Products").Where(whereCond).Get();
            if (result.Count != 1) {
                throw new Exception("Double SKUs");
            }
            foreach (var row in result.Values) {

                prod.DBID = int.Parse(row["ID"]);
                prod.title = row["Title"];
                prod.body_html = row["Body"];
                prod.vendor = row["Vendor"];
                prod.product_type = categoriesKVP[row["ProductType"]];
                prod.price = double.Parse(row["Price"]);
                prod.sku = row["SKU"];
                prod.stock = int.Parse(row["Stock"]);
                prod.barcode = row["Barcode"];
                prod.vendor_price = double.Parse(row["PriceVendor"]);
                prod.weight = double.Parse(row["Weight"]);
                prod.height = int.Parse(row["Height"]);
                prod.lenght = int.Parse(row["Lenght"]);
                prod.width = int.Parse(row["Width"]);
            }


            //getting images faster
            db = new();
            whereCond = new Dictionary<string, Dictionary<string, string>> {
                ["ProductID"] = new Dictionary<string, string> {
                    ["="] = prod.DBID.ToString()
                }
            };
            result = db.Table(tablePrefix + "_Images").Where(whereCond).Get();
            foreach (var imgRow in result.Values) {
                string imageUrl = imgRow["ImgUrl"];
                prod.images.Add(imageUrl);
            }

            //getting tags faster
            db = new();
            whereCond = new Dictionary<string, Dictionary<string, string>> {
                ["ProductID"] = new Dictionary<string, string> {
                    ["="] = prod.DBID.ToString()
                }
            };
            result = db.Table(tablePrefix + "_Tags").Where(whereCond).Get();
            foreach (var tagRow in result.Values) {
                string tag = tagRow["Tag"];

                prod.tags.Add(tag);
            }

            return prod;
        }

        public static void MarkProductForShopSync(string sku) {

            DataBaseInterface db = new();
            var updateData = new Dictionary<string, string> {
                ["LastUpdateTime"] = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString(),
                ["Status"] = ProductStatus.WaitingShopSync
            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>> {
                ["SKU"] = new Dictionary<string, string> {
                    ["="] = sku
                }
            };
            db.Table("Products").Where(whereUpdate).Update(updateData);
        }

        private static List<Product> GetTDBProducts() {
            List<Product> products = new();
            Dictionary<string, string> categoriesKVP = GetCategoriesDictionary();

            //getting main product info
            DataBaseInterface db = new();
            var result = db.Table("TDB_Products").Get();
            foreach (var prod in result.Values) {

                Product NewProduct = new();
                NewProduct.DBID = int.Parse(prod["ID"]);
                NewProduct.title = prod["Title"];
                NewProduct.body_html = prod["Body"];
                NewProduct.vendor = prod["Vendor"];
                NewProduct.product_type = categoriesKVP[prod["ProductType"]];
                NewProduct.price = double.Parse(prod["Price"]);
                NewProduct.sku = prod["SKU"];
                NewProduct.stock = int.Parse(prod["Stock"]);
                NewProduct.barcode = prod["Barcode"];
                NewProduct.vendor_price = double.Parse(prod["PriceVendor"]);
                NewProduct.weight = double.Parse(prod["Weight"]);
                NewProduct.height = int.Parse(prod["Height"]);
                NewProduct.lenght = int.Parse(prod["Lenght"]);
                NewProduct.width = int.Parse(prod["Width"]);

                products.Add(NewProduct);
            }

            //getting images faster
            db = new();
            result = db.Table("TDB_Images").Get();
            foreach (var imgRow in result.Values) {

                int productID = int.Parse(imgRow["ProductID"]);
                string imageUrl = imgRow["ImgUrl"];

                products.Find(x => x.DBID == productID).images.Add(imageUrl);
            }

            //getting tags faster
            db = new();
            result = db.Table("TDB_Tags").Get();
            foreach (var tagRow in result.Values) {

                int productID = int.Parse(tagRow["ProductID"]);
                string tag = tagRow["Tag"];

                products.Find(x => x.DBID == productID).tags.Add(tag);
            }

            return products;
        }

        public static List<Product> GetAllProducts() {
            List<Product> p = new();

            List<Product> TDBproducts = GetTDBProducts();
            p.AddRange(TDBproducts);
            
            return p;
        }
    }
}