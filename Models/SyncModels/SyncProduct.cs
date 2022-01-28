namespace Ikrito_Fulfillment_Platform.Models {
    class SyncProduct {

        public string sku { set; get; }
        public string productType_ID { set; get; }
        public string productTypeVal { set; get; }
        public string status { set; get; }
        public string lastSyncTime { set; get; }
        public string lastUpdateTime { set; get; }
        public string shopifyID { set; get; }
        public string shopifyVariantID { set; get; }
        public string inventoryItemID { set; get; }

        public SyncProduct() {
            sku = "TEST";
            status = "NEW";
            lastSyncTime = "1";
            lastUpdateTime = "1";
        }

    }
}
