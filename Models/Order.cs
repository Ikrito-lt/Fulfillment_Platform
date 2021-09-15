using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Models {
    public class Order {

        public string id { set; get; }
        public string cancelled_at { set; get; }
        public string closed_at { set; get; }
        public bool confirmed { set; get; }
        public string contact_email { set; get; }
        public string created_at { set; get; }
        public string currency { set; get; }
        public double current_subtotal_price { set; get; }
        public double current_total_discounts { set; get; }
        public double current_total_price { set; get; }
        public double current_total_tax { set; get; }
        public string financial_status { set; get; }
        public string name { set; get; }
        public string note { set; get; }
        public string phone { set; get; }
        public Address billing_address { set; get; }
        public Customer customer { set; get; }
        public List<OrderProduct> line_items { set; get; }
        public Address shipping_address { set; get; }

        public string Item_count => line_items.Count == 1 ? line_items.Count.ToString() + " Item" : line_items.Count.ToString() + " Items";
        public string Full_price_formated => currency == "EUR" ? $"€ {current_total_price:0.00}" : $"{current_total_price:0.00}";

        public string Created_date {
            get {
                DateTime createdDate = DateTime.Parse(created_at);
                return createdDate.ToString("MM/dd/yyyy HH:mm");
            }
        }
    }


    public class Address {
        public string first_name { set; get; }
        public string address1 { set; get; }
        public string phone { set; get; }
        public string city { set; get; }
        public string zip { set; get; }
        public string province { set; get; }
        public string country { set; get; }
        public string last_name { set; get; }
        public string address2 { set; get; }
        public string company { set; get; }
        public string latitude { set; get; }
        public string longditude { set; get; }
        public string name { set; get; }
        public string country_code { set; get; }
        public string province_code { set; get; }
    }
}
