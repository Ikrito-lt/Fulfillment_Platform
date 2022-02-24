using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Ikrito_Fulfillment_Platform.Models {
    /// <summary>
    /// class for storring full product module
    /// </summary>
    public class FullProduct {
        public string Title { set; get; }
        public string HTMLBody { set; get; }           //desc
        public string Vendor { set; get; }
        public string ProductTypeID { set; get; }        //category ID
        public string SKU { set; get; }
        public double Weight { set; get; }              //in kg
        public int Height { set; get; }                 //in mm
        public int Lenght { set; get; }                 //in mm
        public int Width { set; get; }                  //in mm
        public string ProductTypeVendor { set; get; }   //for saving vendor product type to database.
        public string DeliveryTime { set; get; }        //delivery time string
        public string AddedTimeStamp { set; get; }      //timestamp of when product was created
        public DateTime GetAddedTime() {

            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            double timestamp = double.Parse(AddedTimeStamp);
            dateTime = dateTime.AddSeconds(timestamp).ToLocalTime();
            return dateTime;
        }

        public List<string> Tags = new();
        public List<string> Images = new();
        public List<ProductVariant> ProductVariants = new();

        public Dictionary<string, string> ProductAttributtes = new();

        //for util
        public string Status { set; get; }              //for product status as in sync Status
        public string ProductTypeDisplayVal { set; get; }   // for saving category text val

        //for showing 
        public string VariantCount => ProductVariants?.Count > 0 ? ProductVariants.Count.ToString() : "0";
        public string FirstVariantStock => ProductVariants?.Count > 0 ? ProductVariants.First().Stock.ToString() : "NaN";
        public string FirstVariantPrice => ProductVariants?.Count > 0 ? ProductVariants.First().Price.ToString() : "NaN";
        public string FirstVariantVendorPrice => ProductVariants?.Count > 0 ? ProductVariants.First().PriceVendor.ToString() : "NaN";

        /// <summary>
        /// class for describing product variants
        /// </summary>
        public class ProductVariant {
            public int VariantDBID { set; get; }
            public double Price { set; get; }
            public int Stock { set; get; }
            public string Barcode { set; get; }
            public double PriceVendor { set; get; }
            public string VariantType { set; get; }
            public string VariantData { set; get; }
            public bool PermPrice { set; get; }

            public bool isSame(ProductVariant other) {
                if(Barcode != other.Barcode) return false;
                if(PriceVendor != other.PriceVendor) return false;
                if(Stock != other.Stock) return false;
                return true;
            }
        }

        public FullProduct() {
            //to detect unassigned fields
            Title = "NULL";
            HTMLBody = "NULL";
            Vendor = "NULL";
            ProductTypeID = "NULL";
            SKU = "NULL";
            Weight = 0;
            Height = 1;
            Lenght = 1;
            Width = 1;
            DeliveryTime = "-";
        }

        //
        // Left after shopify
        //

        //private static string repairHTLMBody(string body) {
        //    //StringBuilder builder = new(body);
        //    //builder.Replace("&#xA;", "\\n");
        //    //builder.Replace("\"", "\\\"");
        //    //string s = builder.ToString();

        //    //s = Regex.Replace(s, @"\r\n?|\n", "");

        //    StringWriter wr = new StringWriter();
        //    var jsonWriter = new JsonTextWriter(wr);
        //    jsonWriter.StringEscapeHandling = StringEscapeHandling.EscapeHtml;
        //    new JsonSerializer().Serialize(jsonWriter, body);
        //    var d = wr.ToString();
        //    return d;
        //}

        //private static string repairTitle(string title) {

        //    StringBuilder builder = new(title);
        //    builder.Replace("\"", "\\\"");
        //    builder.Replace("\n", "");
        //    return builder.ToString();
        //}

        //public string GetImportJsonString() {

        //    string imagesStr = "";
        //    if (images.Count != 0) {
        //        foreach (var image in images) {
        //            imagesStr += $@"{{""src"": ""{image}""}},";
        //        }
        //        imagesStr = imagesStr.Remove(imagesStr.Length - 1);
        //    }

        //    string tagsStr = "";
        //    if (tags.Count != 0) {
        //        foreach (var tag in tags) {
        //            tagsStr += $@"""{tag}"",";
        //        }
        //        tagsStr = tagsStr.Remove(tagsStr.Length - 1);
        //    }

        //    //todo: chnage this hack
        //    //string retString =
        //    //@$"{{
        //    //    ""product"": {{
        //    //        ""title"": ""{repairTitle(title)}"",
        //    //        ""body_html"": {repairHTLMBody(body_html)},
        //    //        ""vendor"": ""{vendor}"",
        //    //        ""status"": ""active"",
        //    //        ""product_type"": ""{ProductTypeDisplayVal}"",
        //    //        ""variants"": [
        //    //            {{
        //    //                ""price"": ""{price}"",
        //    //                ""sku"": ""{sku}"",
        //    //                ""weight"": ""{weight}"",
        //    //                ""inventory_quantity"": ""{stock}"",
        //    //                ""barcode"": ""{barcode}""
        //    //            }}
        //    //        ],
        //    //        ""images"": [
        //    //            {imagesStr}
        //    //        ],
        //    //        ""tags"": [
        //    //            {tagsStr}
        //    //        ]
        //    //    }}
        //    //}}";

        //    string retString =
        //    @$"{{
        //        ""product"": {{
        //            ""title"": ""{repairTitle(title)}"",
        //            ""body_html"": {repairHTLMBody(body_html)},
        //            ""vendor"": ""{vendor}"",
        //            ""status"": ""active"",
        //            ""product_type"": ""{ProductTypeDisplayVal}"",
        //            ""variants"": [
        //                {{
        //                    ""price"": ""{productVariants.First().price}"",
        //                    ""sku"": ""{sku}"",
        //                    ""weight"": ""{weight}"",
        //                    ""inventory_quantity"": ""{productVariants.First().stock}"",
        //                    ""barcode"": ""{productVariants.First().barcode}""
        //                }}
        //            ],
        //            ""images"": [
        //                {imagesStr}
        //            ],
        //            ""tags"": [
        //                {tagsStr}
        //            ]
        //        }}
        //    }}";

        //    return retString;
        //}

        //public string GetHeightMetaBody() {

        //    string value = $"{{\\\"unit\\\": \\\"mm\\\",\\\"value\\\": {this.height}}}";

        //    string metaBody =
        //    @$"{{
        //        ""metafield"": {{
        //            ""namespace"": ""my_fields"",
        //            ""key"": ""dimensions"",
        //            ""value"": ""{value}"",
        //            ""type"": ""dimension""
        //        }}
        //    }}";
        //    return metaBody;
        //}

        //public string GetLenghtMetaBody() {

        //    string value = $"{{\\\"unit\\\": \\\"mm\\\",\\\"value\\\": {this.lenght}}}";

        //    string metaBody =
        //    @$"{{
        //        ""metafield"": {{
        //            ""namespace"": ""my_fields"",
        //            ""key"": ""lenght"",
        //            ""value"": ""{value}"",
        //            ""type"": ""dimension""
        //        }}
        //    }}";
        //    return metaBody;
        //}

        //public string GetWidthMetaBody() {

        //    string value = $"{{\\\"unit\\\": \\\"mm\\\",\\\"value\\\": {this.width}}}";

        //    string metaBody =
        //    @$"{{
        //        ""metafield"": {{
        //            ""namespace"": ""my_fields"",
        //            ""key"": ""width"",
        //            ""value"": ""{value}"",
        //            ""type"": ""dimension""
        //        }}
        //    }}";
        //    return metaBody;
        //}

        //public string GetVendorPriceBody() {
        //    //todo: change this hack
        //    //string vendorPriceBody = $"{{\"inventory_item\": {{\"cost\": \"{vendor_price.ToString()}\"}}}}";
        //    string vendorPriceBody = $"{{\"inventory_item\": {{\"cost\": \"{productVariants.First().vendor_price.ToString()}\"}}}}";

        //    return vendorPriceBody;
        //}
    }
    //TODO: think about adding SEO shit
}
