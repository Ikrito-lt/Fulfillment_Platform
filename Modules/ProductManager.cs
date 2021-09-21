using Ikrito_Fulfillment_Platform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules {
    class ProductManager {

        private readonly string getOrdersEndPoint = "https://real-europe-corp.myshopify.com/admin/api/2021-07/orders.json";

        public void ImportProducts() {
            List<Product> importProducts = new();
            importProducts.Add(new Product());
            importProducts.Add(new Product());





        }


    }
}
