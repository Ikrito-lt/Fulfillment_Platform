using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Utils;
using System.Collections.Generic;

namespace Ikrito_Fulfillment_Platform.Utils
{
    class Test {
        public Test() {
            //testBeFancyImport();
            //testTitles();
            //a();
            //ImportProductTypes();
        }

        private static void ImportProductTypes() {
            DataBaseInterface oldDB = new DataBaseInterface(Globals.oldDBHostname, Globals.oldDBUsername, Globals.oldDBPassword, Globals.oldDefaultDB);
            DataBaseInterface newDB = new DataBaseInterface();

            var currentPTypes = ProductModule.GetCategoriesDictionary();
            var oldPT = oldDB.Table("ProductTypes").Get();
            Dictionary<string, string> oldPTypes = new();
            foreach (var row in oldPT.Values) {

                var id = row["ID"];
                var type = row["ProductType"];

                oldPTypes.Add(id, type);
            }

            var oldSKUs = oldDB.Table("Products").Get("SKU");
            foreach (var oldSKU in oldSKUs) {
                string osku = oldSKU.Value["SKU"];
                string tablePrefix = osku.GetUntilOrEmpty();
                if (tablePrefix == "KG") continue;
                if (ProductModule.CheckIfExistsInDB(osku)) {
                    //if iot exists get old BD product tupe

                    var whereQ = new Dictionary<string, Dictionary<string, string>>
                    {
                        ["SKU"] = new Dictionary<string, string>
                        {
                            ["="] = osku
                        }
                    };
                    var oldProductType = oldDB.Table($"{tablePrefix}_Products").Where(whereQ).Get("ProductType")[0]["ProductType"];
                    string oldPTSRT = oldPTypes[oldProductType];

                    string newPTID = currentPTypes.GetFirstKeyByValue(oldPTSRT);
                    FullProduct p = ProductModule.GetProduct(osku);
                    if (p.productTypeID == "1" && oldProductType != "97")
                    {
                        ProductModule.ChangeProductCategory(osku, newPTID);
                        System.Console.WriteLine($"{oldSKU.Key} / {osku} / {oldPTSRT}");
                    }
                    else { continue; }

                    //string oldPTStr = oldPTypes[oldProductType];


                    //assign cat
                }
            }

            var d = oldSKUs;

        }

        //public static void testBeFancyImport() {
        //    var s = BFModule.ProductList;
        //}

        //private static void testTitles() {
        //    List<Product> pList = ProductModule.GetAllProducts();
        //    List<Product> titleList = new();

        //    foreach (Product p in pList) {
        //        if (p.title.Length > 200) {
        //            titleList.Add(p);
        //        }
        //    }

        //    var a = titleList;
        //}
    }
}
