using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier.Pretendentas.Models {
    public class PDProduct {
        public string id { set; get; }
        public string title { set; get; }
        public string artnum { set; get; }
        public string ean { set; get; }
        public string modified_at { set; get; }
        public string warranty { set; get; }
        public string youtube_url { set; get; }
        public string descriptionHTML { set; get; }
        public string manufacturer_id { set; get; }
        public string type_id { set; get; }

        public double price { set; get; }
        public double? b2b_promo_price { set; get; }
        public double? old_price { set; get; }
        public double? rmk { set; get; }
        public double weight { set; get; }              //in kg i guess
        
        public Dictionary<string,string> stock { set; get; }
        public dynamic attributtes { set; get; }
        public dynamic pictures { set; get; }
        public dynamic categories_id { set; get; }
        public dynamic additional_params { set; get; }

        public string vandorType { set; get; }
    }
}
