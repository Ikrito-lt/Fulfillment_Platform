using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Newtonsoft.Json;

namespace Ikrito_Fulfillment_Platform.Utils
{
    class Test {
        public Test() {
            uploadJsonToDB();
        }

        private static void uploadJsonToDB() {
            //using (StreamReader r = new StreamReader(@"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\ApplicationResources\CatTree.json"))
            //{
            //    string json = r.ReadToEnd();
            //    var catTree = JsonConvert.DeserializeObject<CategoryTree>(json);

            //    var serializedCategoryTree = JsonConvert.SerializeObject(catTree);
            //    var escapedSerializedCategoryTree = JsonConvert.ToString(serializedCategoryTree);

            //    DataBaseInterface db = new();
            //    var InsertData = new Dictionary<string, string>
            //    {
            //        ["Json"] = escapedSerializedCategoryTree
            //    };
            //    db.Table("CategoryTreeJson").Insert(InsertData);

            //    Console.WriteLine(json);
            //}

            CategoryTreeModule c = new();
            

        }
    }
}
