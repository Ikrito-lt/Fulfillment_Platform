using System.Collections.Generic;
using System.Text;

namespace Ikrito_Fulfillment_Platform.Models {
    public class Product {

        public int DBID { set; get; }
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
        public bool needsUpdate { set; get; }

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

        private string ShortTitle(string title) {
            // max product name lebht in shopify is 255
            //TODO: remove this hack
            if (title.Length > 255) {
                return title.Trim().Substring(0, 255);
            } else {
                return title;
            }
        }

        private string repairHTLMBody(string body) {

            StringBuilder builder = new StringBuilder(body);
            builder.Replace("&#xA;", "\\n");
            builder.Replace("\n", "\\n");
            builder.Replace("\"", "\\\"");

            return builder.ToString();
        }

        private string repairTitle(string title) {

            StringBuilder builder = new StringBuilder(title);
            builder.Replace("\"", "\\\"");

            return builder.ToString();
        }

        public string GetImportJsonString() {

            string imagesStr = "";
            if (images.Count != 0) {
                foreach (var image in images) {
                    imagesStr += $@"{{""src"": ""{image}""}},";
                }
                imagesStr = imagesStr.Remove(imagesStr.Length - 1);
            }

            string tagsStr = "";
            if (tags.Count != 0) {
                foreach (var tag in tags) {
                    tagsStr += $@"""{tag}"",";
                }
                tagsStr = tagsStr.Remove(tagsStr.Length - 1);
            }

            string retString =
            @$"{{
                ""product"": {{
                    ""title"": ""{ShortTitle(repairTitle(title))}"",
                    ""body_html"": ""{repairHTLMBody(body_html)}"",
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

}
