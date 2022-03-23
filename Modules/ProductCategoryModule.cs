using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules
{
    internal class ProductCategoryModule
    {
        //this singleton is used to retrieve and store product categories
        private readonly Lazy<Dictionary<string, string>> _LazyCategoryKVP = new(() => GetCategoriesDictionary());
        public Dictionary<string, string> CategoryKVP => _LazyCategoryKVP.Value;

        public static ProductCategoryModule Instance { get; private set; }
        static ProductCategoryModule()
        {
            Instance = new ProductCategoryModule();
        }

        //
        // section for product category manipulation
        //

        /// <summary>
        /// changes product category in the database
        /// </summary> 
        /// <param name="sku"></param>
        /// <param name="newCategoryID"></param>
        public static void ChangeProductCategory(string sku, string newCategoryID)
        {
            DataBaseInterface db = new();
            string tablePrefix = sku.GetBeginingOrEmpty();

            //updating *_Products table
            var updateData = new Dictionary<string, string>
            {
                ["ProductType_ID"] = newCategoryID
            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            db.Table($"_{tablePrefix}_Products").Where(whereUpdate).Update(updateData);
            db.Table("Products").Where(whereUpdate).Update(updateData);
        }

        /// <summary>
        /// gets Categories KVP from database
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, string> GetCategoriesDictionary()
        {
            //getting category KVP from database
            Dictionary<string, string> categoriesKVP = new();
            DataBaseInterface db = new();

            var result = db.Table("ProductTypes").Get("ID, ProductType");
            foreach (var cat in result.Values)
            {

                var id = cat["ID"];
                var type = cat["ProductType"];

                categoriesKVP.Add(id, type);
            }
            return categoriesKVP;
        }

    }
}
