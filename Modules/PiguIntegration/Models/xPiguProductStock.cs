using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules.PiguIntegration.Models
{
    internal class xPiguProductStock
    {
        public string PiguSupplierCode { get; set; }
        public string Barcode { get; set; }
        public string PriceBDiscount { get; set; }
        public string PriceADiscount { get; set; }
        public string OurStock { get; set; }
        public string CollectionHours { get; set; }

        public string GetXml()
        {
            string s = $@" 
                <product>
                    <sku>{PiguSupplierCode}</sku>
                    <ean>{Barcode}</ean>
                    <price-before-discount>{PriceBDiscount}</price-before-discount>
                    <price-after-discount>{PriceADiscount}</price-after-discount>
                    <stock>{OurStock}</stock>
                    <collectionhours>{CollectionHours}</collectionhours>
                </product>";
            return s;
        }
    }
}
