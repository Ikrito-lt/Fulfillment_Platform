using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules.PiguIntegration.Models
{
    class PiguProductOffer
    {
        public string SKU { get; set; }
        public string Title { get; set; }
        public string ProductTypeVal { get; set; }
        public string ProductTypeID { get; set; }
        public bool Selected { get; set; }

        public List<PiguVariantOffer> PiguVariantOffers = new();

        public bool AnyVariantOffersEnabled => PiguVariantOffers.Any(x => x.IsEnabled == true);
    }
}
