using Ikrito_Fulfillment_Platform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ikrito_Fulfillment_Platform.Utils;

namespace Ikrito_Fulfillment_Platform.Modules {
    class ProductExporter {

        public static string BaseUrl = "https://real-europe-corp.myshopify.com/admin/api/2021-07/";
        public static string Path = "products.json";

        private readonly List<Product> ExportProducts;
        private readonly RESTClient restClient;
        public delegate void UpdateLoadingBar(string msg, int val);

        public ProductExporter(List<Product> products) {
            ExportProducts = products;
            restClient = new(BaseUrl);
        }

        private void exportProduct(Product product) { 
            
        }

        public void exportProducts(UpdateLoadingBar loadingBar) {

            for (int i = 0; i < 2; i++) {

                exportProduct(ExportProducts[i]);
                loadingBar(msg: i.ToString(), i.Remap(0, ExportProducts.Count, 0, 100));
            }

        }

    }
}
