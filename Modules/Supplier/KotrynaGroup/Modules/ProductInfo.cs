﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier.KotrynaGroup.Modules {
    class ProductInfo {
        public string axapta_id { set; get; }
        public string kpn { set; get; }
        public string coo { set; get; }

        public string title { set; get; }
        public string vendorType { set; get; }
        public string brand { set; get; }

        public Dictionary<string, string> properties = new();
        public List<string> images = new();
    }
}
