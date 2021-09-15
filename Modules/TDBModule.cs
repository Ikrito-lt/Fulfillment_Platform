using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ikrito_Fulfillment_Platform.Modules {
    class TDBModule {

        private static readonly string TDBDesc_location = "C:\\Users\\Luke\\Desktop\\Ikrito_Fulfillment_Platform\\Files\\TDB_cat.xml";
        private static readonly string TDBCat_location = "C:\\Users\\Luke\\Desktop\\Ikrito_Fulfillment_Platform\\Files\\TDB_cat2.xml";

        XmlDocument TDB_desc = new();
        XmlDocument TDB_cat = new();

        public TDBModule() {
            TDB_desc.Load(TDBDesc_location);
            TDB_cat.Load(TDBCat_location);
        }

        public void PushToDB() {

            var TDB_desc_products = TDB_desc.ChildNodes.Item(0).ChildNodes;
            var TDB_cat_products = TDB_cat.ChildNodes.Item(0).ChildNodes;

            Debug.WriteLine($"desc count: {TDB_desc_products.Count}");
            Debug.WriteLine($"desc count: {TDB_cat_products.Count}");


        }

    }
}
