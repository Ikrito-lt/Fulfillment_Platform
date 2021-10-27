using System.Collections.Generic;

namespace Ikrito_Fulfillment_Platform.Modules {
    class TDBModule {

        private Dictionary<string, string> APIParams = new Dictionary<string, string> {
            ["orgnum"] = "268230",
            ["username"] = "PREKES",
            ["pwd"] = "Welcome.123",
            ["ean"] = "y"
        };

        public static string BaseUrl = "http://tdonline.tdbaltic.net/pls/PROD/";
        public static string CataloguePath = "ixml.ProdCatExt";
        public static string DataSheetsPath = "ixml.DSheets";

        //private readonly string TDBDesc_location = @"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Files\TDB\TDB_cat.xml";
        //private readonly string TDBCat_location = @"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Files\TDB\TDB_cat2.xml";
        //private readonly string TDBCategoriesJson = @"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Files\TDB\TDBCategories.json";

        //downloads Catalogue from TDB API
        private string GetAPICatalogue() {
            Dictionary<string, string> catalogueParams = APIParams;
            RESTClient restClient = new(BaseUrl);
            return restClient.ExecGet(CataloguePath, catalogueParams);
        }
    }
}
