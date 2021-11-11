using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Utils {
    static class ProductStatus {
        //when adding new statuses update productMOdule ChangeProductStatus method

        public static string WaitingShopSync = "Waiting Shop Sync";
        public static string New = "New";
        public static string Ok = "Ok";

        //todo: add those cases to sync module
        public static string NeedsArchiving = "Needs Archiving";
        public static string Archived = "Archived";
    }
}
