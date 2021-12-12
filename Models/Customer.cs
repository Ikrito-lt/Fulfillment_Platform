using System;

namespace Ikrito_Fulfillment_Platform.Models {
    public class Customer {
        //shopify_ID
        public string id { set; get; }
        //DB_ID
        public string DBID { set; get; }

        public string email { set; get; }
        public string phone { set; get; }

        public string first_name { set; get; }
        public string last_name { set; get; }
        public string full_name {
            get {
                return first_name + " " + last_name;
            }
        }

        public string created_at { set; get; }
        public string Created_date {
            get {
                if (created_at != null) {
                    DateTime createdDate = DateTime.Parse(created_at);
                    return createdDate.ToString("MM/dd/yyyy HH:mm");
                } else {
                    return "---";
                }
            }
        }

        public string updated_at { set; get; }
        public string Updated_date {
            get {
                if (updated_at != null) {
                    DateTime updatedDate = DateTime.Parse(updated_at);
                    return updatedDate.ToString("MM/dd/yyyy HH:mm");
                } else {
                    return "---";
                }
            }
        }

        public double total_spent { set; get; }
        public string spent_formated => $"€ {total_spent:0.00}";

        public string state { set; get; }
        public int orders_count { set; get; }
        public string last_order_id { set; get; }
        public string last_order_name { set; get; }

        public bool verified_email { set; get; }
        public string note { set; get; }
    }
}
