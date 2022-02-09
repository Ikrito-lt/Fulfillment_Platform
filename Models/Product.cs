 using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Ikrito_Fulfillment_Platform.Models {
    public class Product {

        public string title { set; get; }
        public string body_html { set; get; }           //desc
        public string vendor { set; get; }
        public string productTypeID { set; get; }        //category ID
        public string sku { set; get; }
        public double weight { set; get; }              //in kg
        public int height { set; get; }                 //in mm
        public int lenght { set; get; }                 //in mm
        public int width { set; get; }                  //in mm
        public string deliveryTime { set; get; }        //delivery time string
        public string addedTimeStamp { set; get; }      //timestamp of when product was created
        public string productTypeVendor { set; get; }   //for saving vendor product type to database.

        public List<string> tags = new();
        public List<string> images = new();
        public List<ProductVariant> productVariants = new();

        public Dictionary<string, string> productAttributtes = new();

        //for util
        public string status { set; get; }              //for product status as in sync Status
        public string ProductTypeDisplayVal { set; get; }   // for saving category text val

        //for showing 
        public string VariantCount => productVariants?.Count > 0 ? productVariants.Count.ToString() : "0";
        public string FirstVariantStock => productVariants?.Count > 0 ? productVariants.First().stock.ToString() : "NaN";
        public string FirstVariantPrice => productVariants?.Count > 0 ? productVariants.First().price.ToString() : "NaN";
        public string FirstVariantVendorPrice => productVariants?.Count > 0 ? productVariants.First().vendor_price.ToString() : "NaN";

        /// <summary>
        /// class for describing product variants
        /// </summary>
        public class ProductVariant {
            public int variantDBID { set; get; }
            public double price { set; get; }
            public int stock { set; get; }
            public string barcode { set; get; }
            public double vendor_price { set; get; }
            public string VariantType { set; get; }
            public string VariantData { set; get; }
            public bool PermPrice { set; get; }

            public bool isSame(ProductVariant other) {
                if(barcode != other.barcode) return false;
                if(vendor_price != other.vendor_price) return false;
                if(stock != other.stock) return false;
                return true;
            }
        }

        public Product() {
            //to detect unassigned fields
            title = "NULL";
            body_html = "NULL";
            vendor = "NULL";
            productTypeID = "NULL";
            sku = "NULL";
            weight = 1;
            height = 1;
            lenght = 1;
            width = 1;
            deliveryTime = "";
        }

        private static string repairHTLMBody(string body) {
            //StringBuilder builder = new(body);
            //builder.Replace("&#xA;", "\\n");
            //builder.Replace("\"", "\\\"");
            //string s = builder.ToString();

            //s = Regex.Replace(s, @"\r\n?|\n", "");

            StringWriter wr = new StringWriter();
            var jsonWriter = new JsonTextWriter(wr);
            jsonWriter.StringEscapeHandling = StringEscapeHandling.EscapeHtml;
            new JsonSerializer().Serialize(jsonWriter, body);
            var d = wr.ToString();
            return d;
        }

        private static string repairTitle(string title) {

            StringBuilder builder = new(title);
            builder.Replace("\"", "\\\"");
            builder.Replace("\n", "");
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

            //todo: chnage this hack
            //string retString =
            //@$"{{
            //    ""product"": {{
            //        ""title"": ""{repairTitle(title)}"",
            //        ""body_html"": {repairHTLMBody(body_html)},
            //        ""vendor"": ""{vendor}"",
            //        ""status"": ""active"",
            //        ""product_type"": ""{ProductTypeDisplayVal}"",
            //        ""variants"": [
            //            {{
            //                ""price"": ""{price}"",
            //                ""sku"": ""{sku}"",
            //                ""weight"": ""{weight}"",
            //                ""inventory_quantity"": ""{stock}"",
            //                ""barcode"": ""{barcode}""
            //            }}
            //        ],
            //        ""images"": [
            //            {imagesStr}
            //        ],
            //        ""tags"": [
            //            {tagsStr}
            //        ]
            //    }}
            //}}";

            string retString =
            @$"{{
                ""product"": {{
                    ""title"": ""{repairTitle(title)}"",
                    ""body_html"": {repairHTLMBody(body_html)},
                    ""vendor"": ""{vendor}"",
                    ""status"": ""active"",
                    ""product_type"": ""{ProductTypeDisplayVal}"",
                    ""variants"": [
                        {{
                            ""price"": ""{productVariants.First().price}"",
                            ""sku"": ""{sku}"",
                            ""weight"": ""{weight}"",
                            ""inventory_quantity"": ""{productVariants.First().stock}"",
                            ""barcode"": ""{productVariants.First().barcode}""
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

        public string GetHeightMetaBody() {

            string value = $"{{\\\"unit\\\": \\\"mm\\\",\\\"value\\\": {this.height}}}";

            string metaBody =
            @$"{{
                ""metafield"": {{
                    ""namespace"": ""my_fields"",
                    ""key"": ""dimensions"",
                    ""value"": ""{value}"",
                    ""type"": ""dimension""
                }}
            }}";
            return metaBody;
        }

        public string GetLenghtMetaBody() {

            string value = $"{{\\\"unit\\\": \\\"mm\\\",\\\"value\\\": {this.lenght}}}";

            string metaBody =
            @$"{{
                ""metafield"": {{
                    ""namespace"": ""my_fields"",
                    ""key"": ""lenght"",
                    ""value"": ""{value}"",
                    ""type"": ""dimension""
                }}
            }}";
            return metaBody;
        }

        public string GetWidthMetaBody() {

            string value = $"{{\\\"unit\\\": \\\"mm\\\",\\\"value\\\": {this.width}}}";

            string metaBody =
            @$"{{
                ""metafield"": {{
                    ""namespace"": ""my_fields"",
                    ""key"": ""width"",
                    ""value"": ""{value}"",
                    ""type"": ""dimension""
                }}
            }}";
            return metaBody;
        }

        public string GetVendorPriceBody() {
            //todo: change this hack
            //string vendorPriceBody = $"{{\"inventory_item\": {{\"cost\": \"{vendor_price.ToString()}\"}}}}";
            string vendorPriceBody = $"{{\"inventory_item\": {{\"cost\": \"{productVariants.First().vendor_price.ToString()}\"}}}}";

            return vendorPriceBody;
        }
    }
    //TODO: think about adding SEO shit
}
