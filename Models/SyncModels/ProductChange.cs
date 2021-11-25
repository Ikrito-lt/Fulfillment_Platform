using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Models.SyncModels {
    public class ProductChange {
        public string SKU { set; get; }
        public string PriceVendor { set; get; }
        public string Stock { set; get; }
        public string Barcode { set; get; }
        public string Vendor { set; get; }
        public string VendorType { set; get; }

        public Dictionary<string, string> Changes { set; get; }

        public ProductChange() {
            Changes = new();
        }

    }
}
