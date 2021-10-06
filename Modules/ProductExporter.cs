using Ikrito_Fulfillment_Platform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ikrito_Fulfillment_Platform.Utils;
using System.ComponentModel;

namespace Ikrito_Fulfillment_Platform.Modules {
    class ProductExporter {

        public static string BaseUrl = "https://real-europe-corp.myshopify.com/admin/api/2021-07/";
        public static string ProductPath = "products.json";

        private readonly List<Product> ExportProducts;
        private readonly RESTClient ProductClient;

        private string lastSKU = "TDB-TF2215MC-B2";
        private int lastIndex;

        public ProductExporter(List<Product> products) {
            ExportProducts = products;
            ProductClient = new(BaseUrl);

            lastIndex = ExportProducts.FindIndex(x => x.sku == lastSKU);
        }

        private void exportProduct(Product product) {

            var headers = new Dictionary<string, string>(){
                {"Authorization", Globals.getBase64ShopifyCreds()}
            };

            ProductClient.ExecPost(ProductPath, headers, product.GetImportJsonString());
        }

        public void exportProducts(object sender, DoWorkEventArgs e) {
            int prodCount = ExportProducts.Count;

            for (int i = lastIndex; i < prodCount; i++) {

                int progress = i * 100 / prodCount;
                (sender as BackgroundWorker).ReportProgress(progress);
                exportProduct(ExportProducts[i]);
                
            }

        }

    }
}
