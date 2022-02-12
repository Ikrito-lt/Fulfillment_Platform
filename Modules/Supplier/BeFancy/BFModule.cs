using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules.Supplier.BeFancy.Models;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.Xml;
using static Ikrito_Fulfillment_Platform.Models.FullProduct;

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

        // for getting the XML
        private static readonly Lazy<XmlDocument> _LazyVendorXML = new(() => GetVendorXML());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static XmlDocument _VendorXML => _LazyVendorXML.Value;

        //for extracting vendor products
        private static readonly Lazy<List<BFProduct>> _LazyVendorProductList = new(() => GetVendorProductList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static List<BFProduct> _VendorProductList => _LazyVendorProductList.Value;

        //for extracting vendor products that have variants
        private static readonly Lazy<List<BFProductWithVariants>> _LazyVendorProductWithVariantsList = new(() => GetVendorProductWithVariantsList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static List<BFProductWithVariants> _VendorProductWithVariantsList => _LazyVendorProductWithVariantsList.Value;

        private static readonly Lazy<List<FullProduct>> _LazyProductList = new(() => BuildProductList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<FullProduct> ProductList => _LazyProductList.Value;



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
                    throw new Exception("BeFancy changed their XML structure");
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
                List<BFProductVariant> variants = new();
                foreach (XmlNode variant in product.SelectSingleNode("variants").ChildNodes)
                {
                    var varName = variant.Attributes["group_title"];
                    if (varName != null)
                    {
                        BFProductVariant productVariant = new BFProductVariant();
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

        /// <summary>
        /// bulding List of basic products 
        /// </summary>
        /// <returns></returns>
        public static List<FullProduct> BuildProductList()
        {
            List<FullProduct> pList = new();
            //with one variant
            foreach (BFProduct product in _VendorProductList)
            {
                //IMPORTANT: if product doesnt have an id we arent selling it. (it  doesnt get added to our database)
                string ProductID = product.id;
                if (!string.IsNullOrEmpty(ProductID))
                {
                    //add to list
                    pList.Add(BuildProductWithOutVariants(product));
                }
                else {
                    continue;
                }
            }
            //with multiple variants
            foreach (BFProductWithVariants product in _VendorProductWithVariantsList)
            {
                //IMPORTANT: if product doesnt have an id we arent selling it. (it  doesnt get added to our database)
                string ProductID = product.id;
                if (!string.IsNullOrEmpty(ProductID))
                {
                    //add to list
                    pList.Add(BuildProductWithVariants(product));
                }
                else
                {
                    continue;
                }
            }
            return pList;
        }

        /// <summary>
        /// builds Product with one variant from API data
        /// </summary>
        /// <param name="bfProduct"></param>
        /// <returns></returns>
        public static FullProduct BuildProductWithOutVariants(BFProduct bfProduct)
        {
            FullProduct newProduct = new();

            newProduct.title = SQLUtil.SQLSafeString(bfProduct.title);
            newProduct.body_html = SQLUtil.SQLSafeString(bfProduct.description);

            string newManufacturer = bfProduct.manufacturer;
            if (string.IsNullOrEmpty(newManufacturer))
            {
                newManufacturer = "NULL_ERROR";
            }

            newProduct.sku = _SKUPrefix + bfProduct.id;
            newProduct.vendor = SQLUtil.SQLSafeString(newManufacturer);
            newProduct.productTypeID = 1.ToString();
            newProduct.deliveryTime = bfProduct.deliveryTimeText;

            newProduct.productTypeVendor = bfProduct.category;
            newProduct.images = bfProduct.imageURLs;
            newProduct.productAttributtes = bfProduct.attributes;

            //adding only one variant
            ProductVariant productVariant = new ProductVariant();
            productVariant.barcode = bfProduct.barcode;
            productVariant.stock = bfProduct.stock;
            productVariant.vendor_price = Math.Round(bfProduct.price, 2, MidpointRounding.AwayFromZero);
            productVariant.price = Math.Round(PriceGenModule.GenNewPrice(productVariant.vendor_price), 2, MidpointRounding.AwayFromZero);

            newProduct.productVariants.Add(productVariant);

            //no tags in new products;
            //BF doesnt give us dimensions or weight 
            newProduct.height = 1;
            newProduct.width = 1;
            newProduct.lenght = 1;
            newProduct.weight = 1;

            //adding product added timestamp
            newProduct.addedTimeStamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();

            return newProduct;
        }

        // method that builds Product from API models containing multiple variants
        public static FullProduct BuildProductWithVariants(BFProductWithVariants bfProduct)
        {
            FullProduct newProduct = new();

            newProduct.title = SQLUtil.SQLSafeString(bfProduct.title);
            newProduct.body_html = SQLUtil.SQLSafeString(bfProduct.description);

            string newManufacturer = bfProduct.manufacturer;
            if (string.IsNullOrEmpty(newManufacturer))
            {
                newManufacturer = "NULL_ERROR";
            }

            newProduct.sku = _SKUPrefix + bfProduct.id;
            newProduct.vendor = SQLUtil.SQLSafeString(newManufacturer);
            newProduct.productTypeID = 1.ToString();
            newProduct.deliveryTime = bfProduct.deliveryTimeText;

            newProduct.productTypeVendor = bfProduct.category;
            newProduct.images = bfProduct.imageURLs;
            newProduct.productAttributtes = bfProduct.attributes;

            //BF has same prive for all variants
            double vendorPrice = Math.Round(bfProduct.price, 2, MidpointRounding.AwayFromZero);
            double ourPrice = Math.Round(PriceGenModule.GenNewPrice(vendorPrice), 2, MidpointRounding.AwayFromZero);

            //adding multiple variants
            foreach (BFProductVariant variant in bfProduct.variants) {
                ProductVariant productVariant = new ProductVariant();
                productVariant.barcode = variant.barcode;
                productVariant.stock = variant.stock;
                productVariant.VariantType = variant.variantTitle;
                productVariant.VariantData = variant.variantDescription;

                productVariant.vendor_price = vendorPrice;
                productVariant.price = ourPrice;

                newProduct.productVariants.Add(productVariant);
            }

            //no tags in new products;
            //BF doesnt give us dimensions or weight 
            newProduct.height = 1;
            newProduct.width = 1;
            newProduct.lenght = 1;
            newProduct.weight = 1;

            //adding product added timestamp
            newProduct.addedTimeStamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();

            return newProduct;
        }
    }
}
