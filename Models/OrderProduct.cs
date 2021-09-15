using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Models {
    public class OrderProduct {
        public string id { set; get; }
        public int grams { set; get; }
        public string name { set; get; }
        public double price { set; get; }
        public bool product_exists { set; get; }
        public string product_id { set; get; }
        public int quantity { set; get; }
        public string sku { set; get; }
        public bool taxable { set; get; }
        public double total_discount { set; get; }
        public string variant_id { set; get; }
        public string vendor { set; get; }

    }
}
