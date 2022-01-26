using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules.Supplier.BeFancy.Models;
using Ikrito_Fulfillment_Platform.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier.BeFancy
{
    static class BFModule
    {
        //private const string KGApi = Globals.KGApi;
        //private const string KGApiKey = Globals.KGApiKey;

        //private const string _BaseUrl = KGApi + "data/";
        //private static readonly Dictionary<string, string> _APIHeader = new Dictionary<string, string>()
        //{
        //    { "API-KEY", KGApiKey },
        //};

        private const string _SKUPrefix = "BF-";

        //todo: ask why retards have products with out the ids

        // for getting the XML
        private static readonly Lazy<XmlDocument> _LazyVendorXML = new(() => GetVendorXML());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static XmlDocument _VendorXML => _LazyVendorXML.Value;

        //for extracting vendor products
        private static readonly Lazy<List<BFProduct>> _LazyVendorProductList = new(() => GetVendorProductList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<BFProduct> _VendorProductList => _LazyVendorProductList.Value;

        //for extracting vendor products that have variants
        private static readonly Lazy<List<BFProductWithVariants>> _LazyVendorProductWithVariantsList = new(() => GetVendorProductWithVariantsList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<BFProductWithVariants> _VendorProductWithVariantsList => _LazyVendorProductWithVariantsList.Value;

        private static readonly Lazy<List<Product>> _LazyProductList = new(() => BuildProductList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static List<Product> _ProductList => _LazyProductList.Value;

        //private static readonly Lazy<List<Product>> _LazyProductWithVariantsList = new(() => BuildProductWithVariantsList());
        //[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        //private static List<Product> _ProductWithVariantsList => _LazyProductList.Value;


        //
        // section of methods for getting data from KG API
        //

        //gets the product XML from supplier
        private static XmlDocument GetVendorXML() {
            //this will be used to download the xml in the future
            //RESTClient restClient = new(_BaseUrl);
            //string assortmentJson = restClient.ExecGetParams(_CataloguePath, _APIHeader);
            //dynamic assortmentResponse = JsonConvert.DeserializeObject<dynamic>(assortmentJson);

            //loading the xml
            XmlDocument productXML = new();
            productXML.Load(@"C:\Users\Luke\Desktop\Ikrito_Fulfillment_Platform\Modules\Supplier\BeFancy\Models\ikritolt_products_2021-12-30_09_42.xml");
            return productXML;
        }

        //downloads products without variants from BF
        private static List<BFProduct> GetVendorProductList()
        {
            var productList = _VendorXML.SelectNodes("/root/products/product");
            List<BFProduct> vendorProducts = new();
            List<XmlNode> vendorProductsXMLNodes = new();

            foreach (XmlNode product in productList)
            {
                int nodesCount = product.ChildNodes.Count;
                if (nodesCount == 14)
                {
                    vendorProductsXMLNodes.Add(product);
                }
                else if (nodesCount == 13)
                {
                    continue;
                }
                else {
                    throw new Exception("BeFancy chnages their XML structure");
                }
            }

            foreach (XmlNode product in vendorProductsXMLNodes) { 
                BFProduct bFProduct = new BFProduct();
                bFProduct.id = product.SelectSingleNode("id").InnerText.Trim();
                bFProduct.model = product.SelectSingleNode("model").InnerText.Trim();
                bFProduct.category = product.SelectSingleNode("categories").ChildNodes[0].InnerText.Trim();
                bFProduct.title = product.SelectSingleNode("title").InnerText.Trim();
                bFProduct.description = product.SelectSingleNode("description").InnerText.Trim();
                bFProduct.oldPrice = double.Parse(product.SelectSingleNode("price_old").InnerText);
                bFProduct.price = double.Parse(product.SelectSingleNode("price").InnerText);
                bFProduct.manufacturer = product.SelectSingleNode("manufacturer").InnerText.Trim();
                bFProduct.deliveryTimeText = product.SelectSingleNode("delivery_text").InnerText.Trim();
                bFProduct.group = product.SelectSingleNode("group").InnerText.Trim();
                bFProduct.stock = int.Parse(product.SelectSingleNode("quantity").InnerText);
                bFProduct.barcode = product.SelectSingleNode("barcode").InnerText.Trim();

                //adding images
                List<string> imageUrls = new();
                foreach (XmlNode image in product.SelectSingleNode("images").ChildNodes) {
                    imageUrls.Add(image.InnerText);
                }
                bFProduct.imageURLs = imageUrls;

                //adding attributes
                Dictionary<string,string> attributes = new();
                foreach (XmlNode attribute in product.SelectSingleNode("attributes").ChildNodes)
                {
                    var attr = attribute.Attributes["title"];
                    if (attr != null)
                    {
                        attributes.Add(attr.Value, attribute.InnerText);
                    }
                }
                bFProduct.attributes = attributes;

                vendorProducts.Add(bFProduct);
            }
            return vendorProducts;
        }

        //downloads products with variants from BF
        private static List<BFProductWithVariants> GetVendorProductWithVariantsList()
        {
            var productList = _VendorXML.SelectNodes("/root/products/product");
            List<BFProductWithVariants> vendorProductsWithVariants = new();
            List<XmlNode> vendorProductsWithVariantsXMLNodes = new();

            foreach (XmlNode product in productList)
            {
                int nodesCount = product.ChildNodes.Count;
                if (nodesCount == 13)
                {
                    vendorProductsWithVariantsXMLNodes.Add(product);
                }
                else if (nodesCount == 14)
                {
                    continue;
                }
                else
                {
                    throw new Exception("BeFancy chnages their XML structure");
                }
            }

            foreach (XmlNode product in vendorProductsWithVariantsXMLNodes)
            {
                BFProductWithVariants bFProduct = new BFProductWithVariants();
                bFProduct.id = product.SelectSingleNode("id").InnerText.Trim();
                bFProduct.model = product.SelectSingleNode("model").InnerText.Trim();
                bFProduct.category = product.SelectSingleNode("categories").ChildNodes[0].InnerText.Trim();
                bFProduct.title = product.SelectSingleNode("title").InnerText.Trim();
                bFProduct.description = product.SelectSingleNode("description").InnerText.Trim();
                bFProduct.oldPrice = double.Parse(product.SelectSingleNode("price_old").InnerText);
                bFProduct.price = double.Parse(product.SelectSingleNode("price").InnerText);
                bFProduct.manufacturer = product.SelectSingleNode("manufacturer").InnerText.Trim();
                bFProduct.deliveryTimeText = product.SelectSingleNode("delivery_text").InnerText.Trim();
                bFProduct.group = product.SelectSingleNode("group").InnerText.Trim();

                //adding images
                List<string> imageUrls = new();
                foreach (XmlNode image in product.SelectSingleNode("images").ChildNodes)
                {
                    imageUrls.Add(image.InnerText);
                }
                bFProduct.imageURLs = imageUrls;

                //adding attributes
                Dictionary<string, string> attributes = new();
                foreach (XmlNode attribute in product.SelectSingleNode("attributes").ChildNodes)
                {
                    var attr = attribute.Attributes["title"];
                    if (attr != null)
                    {
                        attributes.Add(attr.Value, attribute.InnerText);
                    }
                }
                bFProduct.attributes = attributes;

                //adding variants
                List<ProductVariant> variants = new();
                foreach (XmlNode variant in product.SelectSingleNode("variants").ChildNodes)
                {
                    var varName = variant.Attributes["group_title"];
                    if (varName != null)
                    {
                        ProductVariant productVariant = new ProductVariant();
                        productVariant.variantTitle = varName.Value;
                        productVariant.variantDescription = variant.SelectSingleNode("title").InnerText.Trim();
                        productVariant.barcode = variant.SelectSingleNode("barcode").InnerText.Trim();
                        productVariant.stock = int.Parse(variant.SelectSingleNode("quantity").InnerText.Trim());
                        variants.Add(productVariant);
                    }
                }
                bFProduct.variants = variants;

                vendorProductsWithVariants.Add(bFProduct);
            }
            return vendorProductsWithVariants;
        }


        //
        // Section for getting Product lists from BF product models
        //

        // bulding List of basic products 
        public static List<Product> BuildProductList()
        {
            List<Product> pList = new();
            foreach (BFProduct product in _VendorProductList)
            {
                //IMPORTANT: if product doesnt have an id we arent selling it. (it  doesnt get added to our database)
                string ProductID = product.model;
                if (!string.IsNullOrEmpty(ProductID))
                {
                    //add to list
                    pList.Add(BuildProduct(product));
                }
                else {
                    continue;
                }
            }
            return pList;
        }

        // method that builds Product form API data
        public static Product BuildProduct(BFProduct bfProduct)
        {
            //Product newProduct = new();

            //newProduct.title = SQLUtil.SQLSafeString(PI.title);
            //newProduct.body_html = BuildDescription(PI.properties);

            //string newVendor = PI.brand;
            //if (string.IsNullOrEmpty(newVendor))
            //{
            //    newVendor = "NULL_ERROR";
            //}

            //newProduct.vendor = SQLUtil.SQLSafeString(newVendor);
            //newProduct.product_type = "Not-Assigned";
            //newProduct.price = AP.base_price;
            //newProduct.sku = _SKUPrefix + AP.axapta_id;
            //newProduct.stock = AP.qty;
            //newProduct.barcode = AP.ean;
            //newProduct.vendor_price = AP.price;

            //newProduct.productTypeVendor = PI.vendorType;

            //newProduct.images = PI.images;
            ////no tags in new products;

            ////getting the dimensions
            //newProduct.weight = double.Parse(PM.net_weight);
            //newProduct.height = (int)Math.Round(double.Parse(PM.gross_height) * 1000);
            //newProduct.lenght = (int)Math.Round(double.Parse(PM.gross_depth) * 1000);
            //newProduct.width = (int)Math.Round(double.Parse(PM.gross_width) * 1000);

            //if (newProduct.height == 0) newProduct.height = 1;
            //if (newProduct.width == 0) newProduct.width = 1;
            //if (newProduct.lenght == 0) newProduct.lenght = 1;

            ////adding product added timestamp
            //newProduct.addedTimeStamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();

            return null;
        }

        //    //method that builds description for the product uisng datasheet KVP
        //    private static string BuildDescription(Dictionary<string, string> prodDataKVP)
        //    {
        //        string description = "";
        //        Dictionary<string, string> prodDataKVPNew = new();

        //        foreach (var pair in prodDataKVP)
        //        {
        //            if (pair.Key == pair.Value)
        //            {
        //                description += pair.Key + "<br><br>";
        //            }
        //            else
        //            {
        //                prodDataKVPNew.Add(pair.Key, pair.Value);
        //            }
        //        }

        //        prodDataKVP = prodDataKVPNew;

        //        StringBuilder sb = new();
        //        using (HTMLTable table = new(sb))
        //        {
        //            foreach (var kvp in prodDataKVP)
        //            {
        //                using (HTMLRow row = table.AddRow())
        //                {
        //                    row.AddCell(kvp.Key);
        //                    row.AddCell(kvp.Value);
        //                }
        //            }
        //        }

        //        string finishedTable = sb.ToString();
        //        description += finishedTable;

        //        description = description.Replace("\'", $"\\'");
        //        description = description.Replace("\"", $"\\\"");

        //        return description;
        //    }


        //    //
        //    // Section for automatically updating and adding products to database
        //    //

        //    // method for updates KG Products
        //    public static void UpdateKGProducts(object sender = null, DoWorkEventArgs e = null)
        //    {
        //        List<Product> DBProducts = ProductModule.GetKGProducts();
        //        List<Product> APIProducts = _ProductList;

        //        List<Product> ArchiveProducts = DBProducts.Where(p1 => APIProducts.All(p2 => p2.sku != p1.sku)).ToList();
        //        List<Product> NewProducts = APIProducts.Where(p1 => DBProducts.All(p2 => p2.sku != p1.sku)).ToList();
        //        List<Product> UpdateProducts = APIProducts.Where(p1 => NewProducts.All(p2 => p2.sku != p1.sku)).ToList();

        //        //remove dublicate skus from newProd list
        //        var a = NewProducts.GroupBy(x => x.sku.ToLower()).Where(x => x.LongCount() > 1).ToList();
        //        a.ForEach(x => NewProducts.RemoveAll(y => y.sku.ToLower() == x.Key));

        //        Dictionary<string, Dictionary<string, string>> appliedChanges = new();          //for updates
        //        List<Dictionary<string, string>> newChanges = new();                            //for new products
        //        List<Dictionary<string, string>> archivedChanges = new();                       //for archived Products                     

        //        //archiving products
        //        foreach (Product archiveProduct in ArchiveProducts)
        //        {
        //            try
        //            {
        //                ProductModule.ChangeProductStatus(archiveProduct.sku, ProductStatus.NeedsArchiving);

        //                Dictionary<string, string> archiveChange = new();
        //                archiveChange.Add("SKU", archiveProduct.sku);
        //                archiveChange.Add("PriceVendor", archiveProduct.vendor_price.ToString());
        //                archiveChange.Add("Stock", archiveProduct.stock.ToString());
        //                archiveChange.Add("Barcode", archiveProduct.barcode);
        //                archiveChange.Add("Vendor", archiveProduct.vendor);
        //                archiveChange.Add("VendorType", archiveProduct.productTypeVendor);
        //                archivedChanges.Add(archiveChange);
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        //            }
        //        }

        //        // adding new Products
        //        foreach (Product newProduct in NewProducts)
        //        {
        //            ProductModule.AddProductToDB(newProduct);

        //            Dictionary<string, string> newChange = new();
        //            newChange.Add("SKU", newProduct.sku);
        //            newChange.Add("PriceVendor", newProduct.vendor_price.ToString());
        //            newChange.Add("Stock", newProduct.stock.ToString());
        //            newChange.Add("Barcode", newProduct.barcode);
        //            newChange.Add("Vendor", newProduct.vendor);
        //            newChange.Add("VendorType", newProduct.productTypeVendor);
        //            newChanges.Add(newChange);
        //        }

        //        DataBaseInterface db = new();

        //        //updating products
        //        foreach (Product updateProduct in UpdateProducts)
        //        {
        //            Product oldProduct = DBProducts.Find(x => x.sku == updateProduct.sku);

        //            //if no changes skip
        //            if (updateProduct.stock == oldProduct.stock && updateProduct.vendor_price == oldProduct.vendor_price)
        //            {
        //                continue;
        //            }
        //            else
        //            {

        //                appliedChanges.Add(oldProduct.sku, new Dictionary<string, string>()
        //                {
        //                    ["Stock"] = "",
        //                    ["PriceVendor"] = "",
        //                    ["Price"] = "",
        //                });

        //                //update stock
        //                if (updateProduct.stock != oldProduct.stock)
        //                {
        //                    var stockUpdateData = new Dictionary<string, string>
        //                    {
        //                        ["Stock"] = updateProduct.stock.ToString()
        //                    };
        //                    var stockWhereUpdate = new Dictionary<string, Dictionary<string, string>>
        //                    {
        //                        ["SKU"] = new Dictionary<string, string>
        //                        {
        //                            ["="] = oldProduct.sku
        //                        }
        //                    };
        //                    db.Table("KG_Products").Where(stockWhereUpdate).Update(stockUpdateData);

        //                    //adding change to applied change list
        //                    appliedChanges[oldProduct.sku]["Stock"] = $"{oldProduct.stock} -> {updateProduct.stock}";
        //                }

        //                //update price
        //                if (updateProduct.vendor_price != oldProduct.vendor_price)
        //                {
        //                    //updating price value
        //                    var priceUpdateData = new Dictionary<string, string>
        //                    {
        //                        ["PriceVendor"] = updateProduct.vendor_price.ToString(),
        //                        ["Price"] = updateProduct.price.ToString()
        //                    };
        //                    var priceWhereUpdate = new Dictionary<string, Dictionary<string, string>>
        //                    {
        //                        ["SKU"] = new Dictionary<string, string>
        //                        {
        //                            ["="] = oldProduct.sku
        //                        }
        //                    };
        //                    db.Table("KG_Products").Where(priceWhereUpdate).Update(priceUpdateData);

        //                    //adding change to applied change list
        //                    appliedChanges[oldProduct.sku]["PriceVendor"] = $"{oldProduct.vendor_price} -> {updateProduct.vendor_price}";
        //                    appliedChanges[oldProduct.sku]["Price"] = $"{oldProduct.price} -> {updateProduct.price}";
        //                }

        //                //updating product status
        //                try
        //                {
        //                    ProductModule.ChangeProductStatus(oldProduct.sku, ProductStatus.WaitingShopSync);
        //                }
        //                catch (Exception ex)
        //                {
        //                    MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        //                }
        //            }
        //        }

        //        //pass applied changes and pending changes to update on complete method
        //        Dictionary<string, object> changes = new();
        //        changes.Add("UpdatedProducts", appliedChanges);
        //        changes.Add("ArchivedProducts", archivedChanges);
        //        changes.Add("NewProducts", newChanges);
        //        if (e != null)
        //        {
        //            e.Result = changes;
        //        }
        //    }
    }
}
