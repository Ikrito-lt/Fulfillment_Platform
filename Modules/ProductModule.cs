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

        public Product GetProduct(string sku) {
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

        public List<Product> GetProducts() {
            List<Product> p = new();
            List<Product> TDBproducts = TDBModule.getProductsFromDB();
            // adding all products to one list
            p.AddRange(TDBproducts);

            return p;
        }
    }
}
