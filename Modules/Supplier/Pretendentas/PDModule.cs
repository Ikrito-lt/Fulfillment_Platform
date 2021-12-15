using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules.Supplier.Pretendentas.Models;
using Ikrito_Fulfillment_Platform.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private const string _ManufacturerPath = "manufacturers";

        private const string _SKUPrefix = "PD-";

        private static readonly Lazy<List<PDCategory>> _LazyCategoriesXML = new(() => GetPDCategories());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<PDCategory> _CategoriesXML => _LazyCategoriesXML.Value;

        private static readonly Lazy<List<PDManufacturer>> _LazyManufacturerList = new(() => GetPDManufacturerList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<PDManufacturer> _ManufList => _LazyManufacturerList.Value;

        private static readonly Lazy<List<Product>> _LazyProductList = new(() => BuildProductList());
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
        private static List<Product> BuildProductList() {
            List<PDCategory> CategoryList = _CategoriesXML;
            List<Product> newProducts = new();

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
                    Product Product = BuildProduct(PDproduct);
                    newProducts.Add(Product);
                }
            }

            //removing products without images
            newProducts = newProducts.FindAll(x => x.images.Count > 0);
            
            //remoing product duplicates
            List<Product> noDuplicates = newProducts.GroupBy(x => x.sku).Select(x => x.First()).ToList();
            return noDuplicates;
        }

        //for building product out of PDProduct
        private static Product BuildProduct(PDProduct PDproduct) {
            Product newProduct = new();

            newProduct.VendorID = PDproduct.id;

            newProduct.title = SQLUtil.SQLSafeString(PDproduct.title);
            newProduct.body_html = SQLUtil.SQLSafeString(PDproduct.descriptionHTML);

            //getting vendor
            List<PDManufacturer> ManufList = _ManufList;
            PDManufacturer ProdManuf = ManufList.Find(x => x.id == PDproduct.manufacturer_id);
            newProduct.vendor = SQLUtil.SQLSafeString(ProdManuf.title);

            newProduct.product_type = "Not-Assigned";
            newProduct.price = PriceGenModule.GenNewPrice(PDproduct.price);
            newProduct.sku = _SKUPrefix + PDproduct.artnum;
            newProduct.barcode = PDproduct.ean;
            newProduct.vendor_price = PDproduct.price;

            newProduct.productTypeVendor = PDproduct.vandorType;

            //setting product stock
            //todo: stock has types ask Darka
            if (PDproduct.stock.ContainsKey("type")) {
                if (PDproduct.stock["type"] == "morethan" || PDproduct.stock["type"] == "total") {
                    newProduct.stock = int.Parse(PDproduct.stock["amount"]);
                } else {
                    newProduct.stock = -1;
                }
            } else {
                newProduct.stock = int.Parse(PDproduct.stock["amount"]);
            }

            //adding images
            dynamic imagesDynamic = PDproduct.pictures;
            imagesDynamic = imagesDynamic.big;
            if (imagesDynamic.Type == JTokenType.Array) {
                foreach (dynamic img in imagesDynamic) {
                    string imageURL = img.ToString();
                    newProduct.images.Add(imageURL);
                }
            } else if (imagesDynamic.Type == JTokenType.String) {
                string imageURL = imagesDynamic.Value.ToString();
                newProduct.images.Add(imageURL);
            }


            //adding product added timestamp
            newProduct.addedTimeStamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();

            //getting the dimensions //they dont give dims
            newProduct.weight = PDproduct.weight;
            newProduct.height = 1;
            newProduct.lenght = 1;
            newProduct.width = 1;

            return newProduct;
        }

        //
        // Section for automatically updating and adding products to database
        //

        // method for updates KG Products
        public void UpdatePDProducts(object sender = null, DoWorkEventArgs e = null) {
            List<Product> DBProducts = ProductModule.GetPDProducts();
            List<Product> APIProducts = _ProductList;

            List<Product> ArchiveProducts = DBProducts.Where(p1 => APIProducts.All(p2 => p2.sku != p1.sku)).ToList();
            List<Product> NewProducts = APIProducts.Where(p1 => DBProducts.All(p2 => p2.sku != p1.sku)).ToList();
            List<Product> UpdateProducts = APIProducts.Where(p1 => NewProducts.All(p2 => p2.sku != p1.sku)).ToList();

            //remove dublicate skus from newProd list
            var a = NewProducts.GroupBy(x => x.sku.ToLower()).Where(x => x.LongCount() > 1).ToList();
            a.ForEach(x => NewProducts.RemoveAll(y => y.sku.ToLower() == x.Key));

            Dictionary<string, Dictionary<string, string>> appliedChanges = new();          //for updates
            List<Dictionary<string, string>> newChanges = new();                            //for new products
            List<Dictionary<string, string>> archivedChanges = new();                       //for archived Products                     

            //archiving products
            foreach (Product archiveProduct in ArchiveProducts) {
                try {
                    ProductModule.ChangeProductStatus(archiveProduct.sku, ProductStatus.NeedsArchiving);

                    Dictionary<string, string> archiveChange = new();
                    archiveChange.Add("SKU", archiveProduct.sku);
                    archiveChange.Add("PriceVendor", archiveProduct.vendor_price.ToString());
                    archiveChange.Add("Stock", archiveProduct.stock.ToString());
                    archiveChange.Add("Barcode", archiveProduct.barcode);
                    archiveChange.Add("Vendor", archiveProduct.vendor);
                    archiveChange.Add("VendorType", archiveProduct.productTypeVendor);
                    archivedChanges.Add(archiveChange);
                }
                catch (Exception ex) {
                    MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // adding new Products
            foreach (Product newProduct in NewProducts) {
                ProductModule.AddProductToDB(newProduct);

                Dictionary<string, string> newChange = new();
                newChange.Add("SKU", newProduct.sku);
                newChange.Add("PriceVendor", newProduct.vendor_price.ToString());
                newChange.Add("Stock", newProduct.stock.ToString());
                newChange.Add("Barcode", newProduct.barcode);
                newChange.Add("Vendor", newProduct.vendor);
                newChange.Add("VendorType", newProduct.productTypeVendor);
                newChanges.Add(newChange);
            }

            DataBaseInterface db = new();

            //updating products
            foreach (Product updateProduct in UpdateProducts) {
                Product oldProduct = DBProducts.Find(x => x.sku == updateProduct.sku);

                //if no changes skip
                if (updateProduct.stock == oldProduct.stock && updateProduct.vendor_price == oldProduct.vendor_price) {
                    continue;
                } else {

                    appliedChanges.Add(oldProduct.sku, new Dictionary<string, string>() {
                        ["Stock"] = "",
                        ["PriceVendor"] = "",
                        ["Price"] = "",
                    });

                    //update stock
                    if (updateProduct.stock != oldProduct.stock) {
                        var stockUpdateData = new Dictionary<string, string> {
                            ["Stock"] = updateProduct.stock.ToString()
                        };
                        var stockWhereUpdate = new Dictionary<string, Dictionary<string, string>> {
                            ["SKU"] = new Dictionary<string, string> {
                                ["="] = oldProduct.sku
                            }
                        };
                        db.Table("KG_Products").Where(stockWhereUpdate).Update(stockUpdateData);

                        //adding change to applied change list
                        appliedChanges[oldProduct.sku]["Stock"] = $"{oldProduct.stock} -> {updateProduct.stock}";
                    }

                    //update price
                    if (updateProduct.vendor_price != oldProduct.vendor_price) {
                        //updating price value
                        var priceUpdateData = new Dictionary<string, string> {
                            ["PriceVendor"] = updateProduct.vendor_price.ToString(),
                            ["Price"] = updateProduct.price.ToString()
                        };
                        var priceWhereUpdate = new Dictionary<string, Dictionary<string, string>> {
                            ["SKU"] = new Dictionary<string, string> {
                                ["="] = oldProduct.sku
                            }
                        };
                        db.Table("KG_Products").Where(priceWhereUpdate).Update(priceUpdateData);

                        //adding change to applied change list
                        appliedChanges[oldProduct.sku]["PriceVendor"] = $"{oldProduct.vendor_price} -> {updateProduct.vendor_price}";
                        appliedChanges[oldProduct.sku]["Price"] = $"{oldProduct.price} -> {updateProduct.price}";
                    }

                    //updating product status
                    try {
                        ProductModule.ChangeProductStatus(oldProduct.sku, ProductStatus.WaitingShopSync);
                    }
                    catch (Exception ex) {
                        MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            //pass applied changes and pending changes to update on complete method
            Dictionary<string, object> changes = new();
            changes.Add("UpdatedProducts", appliedChanges);
            changes.Add("ArchivedProducts", archivedChanges);
            changes.Add("NewProducts", newChanges);
            if (e != null) {
                e.Result = changes;
            }
        }








    }
}
