using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier.KotrynaGroup.Modules {
    class KGProductPackaging {

        public string axapta_id { set; get; }
        public List<Packaging> packagings = new();

    }

    public class Packaging {

        public string type_id { set; get; }
        public string type_name { set; get; }
        public string material_code { set; get; }
        public string material_name { set; get; }
        public string weight { set; get; }
        public string unit { set; get; }
        public string qty { set; get; }

    }
}
