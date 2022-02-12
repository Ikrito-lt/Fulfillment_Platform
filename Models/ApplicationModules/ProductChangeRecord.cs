using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Models
{
    internal record ProductChangeRecord
    {
        //record for changes applied to products
        public string SKU { get; init; }//
        public string PriceVendor { get; set; }//
        public string Price { get; set; }//
        public string Stock { get; set; }//
        public string Barcode { get; init; }//
        public string Vendor { get; init; }//
        public string VendorProductType { get; init; }//
        public string ProductType { get; init; }//
        public string Status { get; init; }
        public string VariantData { get; set; }//
        public int VariantCount { get; set; }
        public List<string> ChangesMade { get; set; }

        public string getFormattedSKU => $"{SKU} ({VariantCount}V)";
        public string getChangesMade {
            get {
                string s = "";
                if (ChangesMade.Count > 0)
                {
                    foreach (var change in ChangesMade)
                    {
                        s += $"{change}\n";
                    }
                }
                return s;
            }        
        }
    }
}
