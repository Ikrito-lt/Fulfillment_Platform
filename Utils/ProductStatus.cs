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
        public static string NeedsArchiving = "Needs Archiving";

        //todo: add those cases to sync module
        public static string NeedsUnArchiving = "Needs Un-Archiving";
        public static string Archived = "Archived";


        //method for getting list of values for all declared fields
        public static List<string> GetFields() {
            List<string> fieldValues = new();
            foreach (var fieldInfo in typeof(ProductStatus).GetFields()) {
                string fieldVal = fieldInfo.GetValue(null).ToString();
                fieldValues.Add(fieldVal);
            }
            return fieldValues;
        }
    }
}
