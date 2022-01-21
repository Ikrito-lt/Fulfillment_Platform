using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Modules.Supplier.BeFancy;
using System.Collections.Generic;

namespace Ikrito_Fulfillment_Platform.Utils
{
    class Test {
        public Test() {
            testBeFancyImport();
            //testTitles();
            //a();
        }

        public static void testBeFancyImport() {
            var s = BFModule._VendorProductWithVariantsList;
        }

        private static void a() {
            ProductModule.MarkProductAsNew("KG-2040302-2377");
        }

        private static void testTitles() {
            List<Product> pList = ProductModule.GetAllProducts();
            List<Product> titleList = new();

            foreach (Product p in pList) {
                if (p.title.Length > 200) {
                    titleList.Add(p);
                }
            }

            var a = titleList;
        }
    }
}
