using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Models {
    class Product {

        //{
        //    "product": {
        //        "title": "Burton Custom Freestyle 1512smeta",
        //        "body_html": "<strong>Good snowboard!</strong>",
        //        "vendor": "Burton",
        //        "product_type": "Snowboard",
        //        "variants": [
        //            {
        //                "price": "10.00",
        //                "sku": "123",
        //                "weight": "20",
        //                "inventory_quantity": "20",
        //                "barcode": "sas"
        //            }
        //        ],
        //        "images": [
        //            {
        //                "src": "http://www.tdbaltic.ee/images/ds/BEU3088P-2.jpg"
        //            }
        //        ],
        //        "tags": [
        //            "Barnes & Noble",
        //            "Big Air",
        //            "John's Fav"
        //        ],
        //        "metafields": [
        //            {
        //                "key": "height",
        //                "value": "newvalue",
        //                "value_type": "string",
        //                "namespace": "dimensions"
        //            },
        //            {
        //                "key": "width",
        //                "value": "newvalue",
        //                "value_type": "string",
        //                "namespace": "dimensions"
        //            },
        //            {
        //                "key": "lenght",
        //                "value": "newvalue",
        //                "value_type": "string",
        //                "namespace": "dimensions"
        //            }
        //        ]
        //    }
        //}

        //TODO: think about adding SEO shit
        public string title { set; get; }
        public string body_html { set; get; }    //desc
        public string vendor { set; get; }
        public string product_type { set; get; } //category
        public double price { set; get; }
        public string sku { set; get; }
        public int stock { set; get; }
        public string barcode { set; get; }
        public double vendor_price { set; get; } //TODO: https://community.shopify.com/c/shopify-apis-and-sdks/setting-cost-per-item-via-api/m-p/803984
        public double weight { set; get; }       //in kg
        public int height { set; get; }       //in mm
        public int lenght { set; get; }       //in mm
        public int width { set; get; }        //in mm

        public List<string> tags = new();
        public List<string> images = new();

        public Product() {
            //TODO: testing change it
            title = "test";
            body_html = "test";
            vendor = "test";
            product_type = "test";
            price = 69.69;
            sku = "kill me";
            stock = 69;
            barcode = "kill me too";
            weight = 0;
            height = 0;
            lenght = 0;
            width = 0;
        }



        public string GetImportJsonString() {

            string imagesStr = "";
            foreach (var image in images) {
                imagesStr += $@"{{""src"": ""{image}""}},";
            }
            imagesStr = imagesStr.Remove(imagesStr.Length - 1);

            string tagsStr = "";
            foreach (var tag in tags) {
                tagsStr += $@"""{tag}"",";
            }
            tagsStr = tagsStr.Remove(tagsStr.Length - 1);


            string retString =
            @$"{{
                ""product"": {{
                    ""title"": ""{title}"",
                    ""body_html"": ""{body_html}"",
                    ""vendor"": ""{vendor}"",
                    ""product_type"": ""{product_type}"",
                    ""variants"": [
                        {{
                            ""price"": ""{price}"",
                            ""sku"": ""{sku}"",
                            ""weight"": ""{weight}"",
                            ""inventory_quantity"": ""{stock}"",
                            ""barcode"": ""{barcode}""
                        }}
                    ],
                    ""metafields"": [
                        {{
                            ""key"": ""height"",
                            ""value"": ""{height}"",
                            ""value_type"": ""string"",
                            ""namespace"": ""dimensions""
                        }},
                        {{
                            ""key"": ""width"",
                            ""value"": ""{width}"",
                            ""value_type"": ""string"",
                            ""namespace"": ""dimensions""
                        }},
                        {{
                            ""key"": ""lenght"",
                            ""value"": ""{lenght}"",
                            ""value_type"": ""string"",
                            ""namespace"": ""dimensions""
                        }}
                    ],
                    ""images"": [
                        {imagesStr}
                    ],
                    ""tags"": [
                        {tagsStr}
                    ]
                }}
            }}";    

            return retString;
        }
    }

}
