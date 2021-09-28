using Ikrito_Fulfillment_Platform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules {
    class ProductExporter {

        private readonly List<Product> ExportProducts;

        public ProductExporter(List<Product> products) {
            ExportProducts = products;
        }

        

    }
}
