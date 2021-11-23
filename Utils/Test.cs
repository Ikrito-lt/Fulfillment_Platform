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
            PriceGenModule.GenNewPrice(150.20);
        }
    }
}
