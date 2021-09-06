using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Models {
    public class Customer {
        public string id { set; get; }
        public string email { set; get; }
        public string created_at { set; get; }
        public string updated_at { set; get; }
        public string first_name { set; get; }
        public string last_name { set; get; }
        public int orders_count { set; get; }
        public string state { set; get; }
        public double total_spent { set; get; }
        public string last_order_id { set; get; }
        public string note { set; get; }
        public bool verified_email { set; get; }
        public string phone { set; get; }
        public string last_order_name { set; get; }

    }
}
