using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier.KotrynaGroup.Modules {
    class KGProductMeasurements {

        public string axapta_id { set; get; }
        public string height { set; get; }
        public string width { set; get; }
        public string depth { set; get; }

        public string gross_height { set; get; }
        public string gross_width { set; get; }
        public string gross_depth { set; get; }
        public string net_weight { set; get; }

        public string unit_volume { set; get; }
        public string tare_weight { set; get; }
        public string package_netto_weight { set; get; }

        public string unit { set; get; }
        public string date_added { set; get; }
        public string date_updated { set; get; }

    }
}
