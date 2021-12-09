using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules.Supplier.Pretendentas.Models;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier.Pretendentas {
    class PDModule {

        public const string PDApiKey = Globals.PDApiKey;

        private static readonly Dictionary<string, string> _APIParams = new Dictionary<string, string>()
        {
            { "api_key",  PDApiKey},
        };

        private const string _BaseUrl = "https://api.pretendentas.lt:443/v1/";
        private const string _CategoriesPath = "categories";
        private const string _DataSheetsPath = "ixml.DSheets";

        private const string _CataloguePath = "assortment";                     //get
        private const string _InfoPath = "products_information";                //post
        private const string _MeasurementsPath = "products_measurements";       //post
        private const string _PackagingPath = "products_packaging";             //post
        private const string _HierarchyPath = "hierarchy";                      //post

        private const string _SKUPrefix = "PD-";

        private static readonly Lazy<List<PDCategory>> _LazyCategoriesXML = new(() => GetPDCategories());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<PDCategory> _CategoriesXML => _LazyCategoriesXML.Value;

        private static readonly Lazy<List<Product>> _LazyProductList = new(() => BuildPDProductList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<Product> _ProductList => _LazyProductList.Value;

        //
        // lazy section
        //

        //lazy for getting the categories xml
        private static List<PDCategory> GetPDCategories() {
            RESTClient restClient = new(_BaseUrl);
            string xmlCategoriesStr = restClient.ExecGetParams(_CategoriesPath, _APIParams);

            XmlDocument CategoriesXML = new();
            CategoriesXML.LoadXml(xmlCategoriesStr);
            XmlNodeList catNodes = CategoriesXML.SelectNodes("//items/category");

            List<PDCategory> catList = new();

            foreach (XmlNode cat in catNodes) {
                int catProdCount = int.Parse(cat.SelectSingleNode("products_count").InnerText);
                XmlNodeList SubCategories = cat.SelectNodes("subcategories");

                if (catProdCount > 0) {
                    PDCategory newCat = new();
                    newCat.id = cat.SelectSingleNode("id").InnerText;
                    newCat.title = cat.SelectSingleNode("title").InnerText;
                    newCat.productCount = catProdCount;

                    catList.Add(newCat);
                } 
               
                if ( SubCategories.Count > 0) {
                    foreach (XmlNode subCat in SubCategories) {
                        int subCatProdCount = int.Parse(subCat.SelectSingleNode("products_count").InnerText);
                        if (subCatProdCount > 0) {
                            PDCategory newCat = new();
                            newCat.id = subCat.SelectSingleNode("id").InnerText;
                            newCat.title = subCat.SelectSingleNode("title").InnerText;
                            newCat.productCount = subCatProdCount;

                            catList.Add(newCat);
                        }
                    }
                }

            }
            return catList;
        }

        //lazy for building product list
        private static List<Product> BuildPDProductList() {
            List<PDCategory> CategoryList = _CategoriesXML;
            List<Product> newProducts = new();



            return newProducts;
        }


    }
}
