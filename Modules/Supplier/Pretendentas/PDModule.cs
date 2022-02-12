using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules.Supplier.Pretendentas.Models;
using Ikrito_Fulfillment_Platform.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Xml;
using static Ikrito_Fulfillment_Platform.Models.FullProduct;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier.Pretendentas
{
    static class PDModule {

        private const string PDApiKey = Globals.PDApiKey;

        private static readonly Dictionary<string, string> _APIParams = new Dictionary<string, string>()
        {
            { "api_key",  PDApiKey},
        };

        private const string _BaseUrl = "https://api.pretendentas.lt:443/v1/";
        private const string _CategoriesPath = "categories";
        private const string _ManufacturerPath = "manufacturers";

        private const string _SKUPrefix = "PD-";

        private static readonly Lazy<List<PDCategory>> _LazyCategoriesXML = new(() => GetPDCategories());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static List<PDCategory> _CategoriesXML => _LazyCategoriesXML.Value;

        private static readonly Lazy<List<PDManufacturer>> _LazyManufacturerList = new(() => GetPDManufacturerList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static List<PDManufacturer> _ManufList => _LazyManufacturerList.Value;

        private static readonly Lazy<List<FullProduct>> _LazyProductList = new(() => BuildProductList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<FullProduct> ProductList => _LazyProductList.Value;


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

        //lazy for building manufacturer list
        private static List<PDManufacturer> GetPDManufacturerList() {
            List<PDManufacturer> manufList = new();

            RESTClient restClient = new(_BaseUrl);
            string xmlManufStr = restClient.ExecGetParams(_ManufacturerPath, _APIParams);

            XmlDocument ManufXML = new();
            ManufXML.LoadXml(xmlManufStr);
            XmlNodeList ManufNodes = ManufXML.SelectNodes("//items/manufacturer");

            foreach (XmlNode manufNode in ManufNodes) {
                string manufJSON = JsonConvert.SerializeXmlNode(manufNode);
                manufJSON = manufJSON.Substring(16);
                manufJSON = manufJSON.Remove(manufJSON.Length - 1, 1);
                PDManufacturer PDmanuf = JsonConvert.DeserializeObject<PDManufacturer>(manufJSON);

                manufList.Add(PDmanuf);
            }
            return manufList;
        }

        //lazy for building product list
        private static List<FullProduct> BuildProductList() {
            List<PDCategory> CategoryList = _CategoriesXML;
            List<FullProduct> newProducts = new();

            foreach (PDCategory category in CategoryList) {
                RESTClient restClient = new(_BaseUrl);
                string productsPath = $"{_CategoriesPath}/{category.id}/products/extended";
                string xmlProductString = restClient.ExecGetParams(productsPath, _APIParams);

                XmlDocument ProductsXML = new();
                ProductsXML.LoadXml(xmlProductString);

                if (ProductsXML.SelectSingleNode("//response/success").InnerText == "404") continue;
                XmlNodeList ProductXmlList = ProductsXML.SelectNodes("//items/product");

                foreach (XmlNode productNode in ProductXmlList) {
                    string productJSON = JsonConvert.SerializeXmlNode(productNode);
                    productJSON = productJSON.Substring(11);
                    productJSON = productJSON.Remove(productJSON.Length - 1, 1);
                    PDProduct PDproduct = JsonConvert.DeserializeObject<PDProduct>(productJSON);

                    //getting description
                    XmlNode descNode = productNode.SelectSingleNode("description");
                    PDproduct.descriptionHTML = descNode.InnerText;

                    //getting vendor type
                    string vendorType = category.title;
                    if (vendorType.StartsWith("-")) {
                        vendorType = vendorType.Substring(1);
                    }
                    vendorType.Trim();
                    PDproduct.vandorType = vendorType;

                    //building product from PDProduct
                    FullProduct Product = BuildProduct(PDproduct);
                    newProducts.Add(Product);
                }
            }

            //removing products without images
            newProducts = newProducts.FindAll(x => x.images.Count > 0);
            
            //remoing product duplicates
            List<FullProduct> noDuplicates = newProducts.GroupBy(x => x.sku).Select(x => x.First()).ToList();
            return noDuplicates;
        }

        //for building product out of PDProduct
        private static FullProduct BuildProduct(PDProduct PDproduct) {
            FullProduct newProduct = new();

            newProduct.title = SQLUtil.SQLSafeString(PDproduct.title);
            newProduct.body_html = SQLUtil.SQLSafeString(PDproduct.descriptionHTML);

            //getting vendor
            PDManufacturer ProdManuf = _ManufList.Find(x => x.id == PDproduct.manufacturer_id);
            newProduct.vendor = SQLUtil.SQLSafeString(ProdManuf.title);

            newProduct.productTypeID = 1.ToString();
            newProduct.sku = _SKUPrefix + PDproduct.artnum;
            newProduct.productTypeVendor = PDproduct.vandorType;

            //building Varinats
            ProductVariant newVariant = new ProductVariant();
            newVariant.barcode = PDproduct.ean;
            newVariant.price = PriceGenModule.GenNewPrice(PDproduct.price);
            newVariant.vendor_price = PDproduct.price;

            //setting product variant stock
            if (PDproduct.stock.ContainsKey("type"))
            {
                if (PDproduct.stock["type"] == "morethan" || PDproduct.stock["type"] == "total")
                {
                    newVariant.stock = int.Parse(PDproduct.stock["amount"]);
                }
                else
                {
                    newVariant.stock = -1;
                }
            }
            else
            {
                newVariant.stock = int.Parse(PDproduct.stock["amount"]);
            }

            //adding images
            dynamic imagesDynamic = PDproduct.pictures;
            imagesDynamic = imagesDynamic.big;
            if (imagesDynamic.Type == JTokenType.Array)
            {
                foreach (dynamic img in imagesDynamic)
                {
                    string imageURL = img.ToString();
                    newProduct.images.Add(imageURL);
                }
            }
            else if (imagesDynamic.Type == JTokenType.String)
            {
                string imageURL = imagesDynamic.Value.ToString();
                newProduct.images.Add(imageURL);
            }

            //adding product added timestamp
            newProduct.addedTimeStamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();

            //getting the dimensions (they dont give dims)
            newProduct.weight = PDproduct.weight;
            newProduct.height = 1;
            newProduct.lenght = 1;
            newProduct.width = 1;

            //i can add paramethers but PD doesnt give any
            newProduct.productVariants.Add(newVariant);
            return newProduct;
        }
    }
}
