using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ikrito_Fulfillment_Platform.Models {
    public class Product {

        public int DBID { set; get; }
        public string title { set; get; }
        public string body_html { set; get; }           //desc
        public string vendor { set; get; }
        public string product_type { set; get; }        //category
        public double price { set; get; }
        public string sku { set; get; }
        public int stock { set; get; }
        public string barcode { set; get; }
        public double vendor_price { set; get; } 
        public double weight { set; get; }              //in kg
        public int height { set; get; }                 //in mm
        public int lenght { set; get; }                 //in mm
        public int width { set; get; }                  //in mm

        public string addedTimeStamp { set; get; }    //timestamp of when product was created
        public string productTypeVendor { set; get; }   //for saving vendor product type to database.

        public List<string> tags = new();
        public List<string> images = new();

        public Product() {
            //to detect unassigned fields
            title = "NULL";
            body_html = "NULL";
            vendor = "NULL";
            product_type = "NULL";
            price = -1;
            sku = "NULL";
            stock = -1;
            barcode = "NULL";
            vendor_price = -1;
            weight = -1;
            height = -1;
            lenght = -1;
            width = -1;
        }

        private string repairHTLMBody(string body) {
            StringBuilder builder = new StringBuilder(body);
            builder.Replace("&#xA;", "\\n");
            builder.Replace("\"", "\\\"");
            string s = builder.ToString();

            s = Regex.Replace(s, @"\r\n?|\n", "");
            return s;
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
                    ""title"": ""{repairTitle(title)}"",
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
            string vendorPriceBody = $"{{\"inventory_item\": {{\"cost\": \"{vendor_price.ToString()}\"}}}}";

            return vendorPriceBody;
        }

    }

    //TODO: think about adding SEO shit
}
