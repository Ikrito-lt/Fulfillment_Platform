using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Ikrito_Fulfillment_Platform.Modules.PiguIntegration.Models
{
    internal class PiguVariantOffer
    {
        public string SKU { get; set; }
        public string Barcode { get; set; }
        public string VariantName { get; set; }
        public string PiguSupplierCode { get; set; }
        public string PriceBDiscount { get; set; }
        public string PriceADiscount { get; set; }
        public string OurStock { get; set; }
        public bool IsEnabled { get; set; }
        public string OfferCreatedUnixStamp { get; set; }
        public double PriceADiscoutDouble => double.Parse(PriceADiscount);

        public string CollectionHours = "24";

        public PiguVariantOffer() { }

        public PiguVariantOffer(string sku, string barcode, string variantName, double price, int ourStock) {
            SKU = sku;
            Barcode = barcode;
            VariantName = variantName;
            PriceADiscount = price.ToString();
            PriceBDiscount = price.ToString();
            OurStock =  ourStock.ToString();

            PiguSupplierCode = StrHash.HashString(SKU + Barcode + variantName, 10);
            
            IsEnabled = false;
            DateTime foo = DateTime.Now;
            OfferCreatedUnixStamp = ((DateTimeOffset)foo).ToUnixTimeSeconds().ToString();
        }
    }
}
