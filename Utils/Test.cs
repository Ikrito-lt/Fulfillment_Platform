using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Modules.Supplier;
using Ikrito_Fulfillment_Platform.Modules.Supplier.Pretendentas;
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
            //foo();
            //too();
            //testTitles();
            //PDCat();
            //testOrders();
            //a();
        }

        private static void a() {
            ProductModule.MarkProductAsNew("KG-2040302-2377");
        }

        private static void foo() {
            KGModule KG = new();
            KG.UpdateKGProducts();
        }

        private static void too() {
            TDBModule TDB = new();
            TDB.UpdateTDBProducts();
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

        private static void PDCat() {
            PDModule PDM = new();
            var a = PDModule._ProductList;
        }

        private static void testOrders() {
            OrderModule orderM = new();
            orderM.FulFillOrder(orderM.getNewOrders()[2]);
        }
    }
}
