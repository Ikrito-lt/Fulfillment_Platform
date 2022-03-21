using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;

namespace Ikrito_Fulfillment_Platform.Models
{
    public class Order {
        //shopify_ID
        public string id { set; get; }
        //DB_ID
        public string DBID { set; get; }

        public string contact_email { set; get; }
        public bool confirmed { set; get; }
        public string confirmedString {
            get {
                if (confirmed) {
                    return "True";
                } else {
                    return "False";
                }
            }
        }

        //date stuff
        public string created_at { set; get; }
        public string Created_date {
            get {
                if (!string.IsNullOrEmpty(created_at)) {
                    DateTime createdDate = created_at.UnixTimeToDateTime();
                    return createdDate.ToString("MM/dd/yyyy HH:mm");
                } else {
                    return "---";
                }
            }
        }
      
        public string cancelled_at { set; get; }
        public string Canceled_date {
            get {
                if (!string.IsNullOrEmpty(cancelled_at)) {
                    DateTime CanceledDate = cancelled_at.UnixTimeToDateTime();
                    return CanceledDate.ToString("MM/dd/yyyy HH:mm");
                } else {
                    return "Not Canceled";
                }
            }
        }

        public string closed_at { set; get; }
        public string Closed_date {
            get {
                if (!string.IsNullOrEmpty(closed_at)) {
                    DateTime ClosedDate = closed_at.UnixTimeToDateTime();
                    return ClosedDate.ToString("MM/dd/yyyy HH:mm");
                } else {
                    return "Not Closed";
                }
            }
        }


        //money stuff
        public string currency { set; get; }
        public double current_subtotal_price { set; get; }
        public string subtotal_price_formated => currency == "EUR" ? $"€ {current_subtotal_price:0.00}" : $"{current_subtotal_price:0.00}";

        public double current_total_discounts { set; get; }
        public string discounts_formated => currency == "EUR" ? $"€ {current_total_discounts:0.00}" : $"{current_total_discounts:0.00}";
        
        public double current_total_price { set; get; }
        public string Full_price_formated => currency == "EUR" ? $"€ {current_total_price:0.00}" : $"{current_total_price:0.00}";
        
        public double current_total_tax { set; get; }
        public string tax_formated => currency == "EUR" ? $"€ {current_total_tax:0.00}" : $"{current_total_tax:0.00}";
        public string financial_status { set; get; }
        
        
        public string name { set; get; }
        public string note { set; get; }
        public string phone { set; get; }
        public Address billing_address { set; get; }
        public Customer customer { set; get; }
        public List<OrderProduct> line_items { set; get; }
        public Address shipping_address { set; get; }

        public string Item_count => line_items.Count == 1 ? line_items.Count.ToString() + " Item" : line_items.Count.ToString() + " Items";
        
    }

    public class Address {
        //DB_ID
        public string DBID { set; get; }

        public string first_name { set; get; }
        public string last_name { set; get; }
        public string full_name {
            get {
                return first_name + " " + last_name;
            }
        }

        public string company { set; get; }

        public string address1 { set; get; }
        public string address2 { set; get; }

        public string city { set; get; }
        public string province { set; get; }
        public string country { set; get; }
        public string cpc => $"{city}, {province}, {country}";

        public string zip { set; get; }
        public string country_code { set; get; }
        public string province_code { set; get; }

        public string name { set; get; }
        public string phone { set; get; }

        public string latitude { set; get; }
        public string longditude { set; get; }
    }
}
