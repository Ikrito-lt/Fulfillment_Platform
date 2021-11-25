    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Models {
    class SyncProduct {

        public int id { set; get; }
        public string sku { set; get; }
        public string status { set; get; }
        public string lastSyncTime { set; get; }
        public string lastUpdateTime { set; get; }
        public string shopifyID { set; get; }
        public string shopifyVariantID { set; get; }
        public string inventoryItemID { set; get; }

        public SyncProduct() {
            id = 0;
            sku = "TEST";
            status = "NEW";
            lastSyncTime = "1";
            lastUpdateTime = "1";
        }

    }
}
