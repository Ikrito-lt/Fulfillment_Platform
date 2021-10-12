using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Utils {
    static class SiteNav {

        public static void GoToSite(string url) {
            System.Diagnostics.Process.Start("explorer.exe", url);
        }
    }
}
