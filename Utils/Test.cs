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
            //testTable();
        }

        private void foo() {
            TDBModule TDBUpdater = new();
            //TDBUpdater.updateTDBProducts();

            Dictionary<string, string> testKVP = new() {
                { "SKU", "TDB-RBC6" },
                { "Vendor", "TDB-RBC6" },
                { "Stock", "6" },
                { "Barcode", "TDB-RBC6" },
                { "PriceVendor", "69" }
            };

            TDBUpdater.addNewTDBProduct(testKVP);
        }

        private void testTable() {
            StringBuilder sb = new StringBuilder();

            List<string> l = new();
            l.Add("a");
            l.Add("b");
            l.Add("c");

            using (HTMLTable table = new HTMLTable(sb)) {
                foreach (var ll in l) {
                    using (HTMLRow row = table.AddRow()) {
                        row.AddCell(ll);
                        row.AddCell(ll + ll);
                    }
                }
            }

            string finishedTable = sb.ToString();
        }

    }
}
