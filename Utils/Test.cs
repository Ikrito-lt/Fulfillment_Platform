using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Modules.Supplier;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Utils {
    class Test {
        public Test() {
            //foo();
            //too();
        }

        private static void foo() {
            KGModule KG = new();
            KG.UpdateKGProducts();
        }

        private static void too() {
            TDBModule TDB = new();
            TDB.UpdateTDBProducts();
        }
    }
}
