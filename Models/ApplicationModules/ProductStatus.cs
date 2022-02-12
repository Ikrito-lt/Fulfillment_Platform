using System.Collections.Generic;

namespace Ikrito_Fulfillment_Platform.Models {
    static class ProductStatus {
        //when adding new statuses update productMOdule ChangeProductStatus method

        public static string New = "New";
        public static string Ok = "Ok";
        public static string OutOfStock = "Out Of Stock";
        public static string Disabled = "Disabled";
        public static string Archived = "Archived";

        //left from shopify
        //public static string WaitingShopSync = "Waiting Shop Sync";
        //public static string NeedsArchiving = "Needs Archiving";
        //public static string NeedsUnArchiving = "Needs Un-Archiving";

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
