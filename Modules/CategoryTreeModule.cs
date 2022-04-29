using Ikrito_Fulfillment_Platform.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Ikrito_Fulfillment_Platform.Modules
{
    internal class CategoryTreeModule
    {
        public CategoryTree categoryTree;

        //making this into singleton
        public static CategoryTreeModule Instance { get; private set; }
        static CategoryTreeModule()
        {
            Instance = new CategoryTreeModule();
        }

        //private constructor
        private CategoryTreeModule()
        {
            fetchCategoryTree();
        }

        /// <summary>
        /// method fetches categoryTree json form database and then deserializes it
        /// </summary>
        /// <returns></returns>
        public void fetchCategoryTree() {

            DataBaseInterface db = new();
            var whereFetch = new Dictionary<string, Dictionary<string, string>>
            {
                ["ID"] = new Dictionary<string, string>
                {
                    ["="] = "1"
                }
            };
            var result = db.Table($"CategoryTreeJson").Where(whereFetch).Get();

            //extracting category tree json
            var json = result[0]["Json"];
            json = json.Remove(0, 1);
            json = json.Remove(json.Length - 1, 1);
            var catTree = JsonConvert.DeserializeObject<CategoryTree>(json);
            this.categoryTree = catTree;
        }
    }
}
