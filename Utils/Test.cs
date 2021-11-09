using Ikrito_Fulfillment_Platform.Modules;
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
            foo();
        }

        private void foo() {
            TDBModule TDBUpdater = new();
            //TDBUpdater.updateTDBProducts();

            Dictionary<string, string> testKVP = new() {
                { "SKU", "TDB-RBC6" },
                { "Vendor", "TDB-RBC6" }
            };

            TDBUpdater.addNewTDBProduct(testKVP);
        }
    }
}
