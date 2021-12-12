using Ikrito_Fulfillment_Platform.Utils;

namespace Ikrito_Fulfillment_Platform.Models {
    public class OrderProduct {
        //shopify_ID
        public string id { set; get; }
        //DB_ID
        public string DBID { set; get; }
        public string OrderDBID { set; get; }

        public string sku { set; get; }
        public string name { set; get; }
        public string name_trimmed => name.Trim().Truncate(65);
        public string vendor { set; get; }

        public int quantity { set; get; }
        public string vnt => $"{quantity} vnt.";
        public double price { set; get; }
        public string priceStr => $"€ {price}";
        public string full_price => $"€ {quantity * price}";
        public double total_discount { set; get; }
        public string discountStr => $"€ {total_discount}";

        public int grams { set; get; }

        public bool product_exists { set; get; }
        public bool taxable { set; get; }

        public string product_id { set; get; }
        public string variant_id { set; get; }
    }
}
