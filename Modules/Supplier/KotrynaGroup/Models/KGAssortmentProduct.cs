using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier.KotrynaGroup.Modules {
    class KGAssortmentProduct {

        public int id { set; get; }
        public string axapta_id { set; get; }
        public string ean { set; get; }
        public int qty { set; get; }
        public double base_price { set; get; }
        public double price { set; get; }
        public string discount { set; get; }
        
        public string newProduct { set; get; }

    }
}
