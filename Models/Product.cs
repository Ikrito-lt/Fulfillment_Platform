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
        public string title;
        public string body_html;    //desc
        public string vendor;
        public string product_type; //category
        public double price;
        public string sku;
        public int stock;
        public string barcode;
        public double vendor_price; //TODO: https://community.shopify.com/c/shopify-apis-and-sdks/setting-cost-per-item-via-api/m-p/803984
        public double weight;       //in kg
        public int height;       //in mm
        public int lenght;       //in mm
        public int width;        //in mm

        public List<string> tags = new();
        public List<string> images = new();

        public Product() {
            //TODO: testing change it
            title = "Burton Custom Freestyle 1512smeta";
            body_html = "<strong>Good snowboard!</strong>";
            vendor = "burton";
            product_type = "Snowboard";
            price = 23.99;
            sku = "kill me";
            stock = 24;
            barcode = "kill me too";
            weight = 420.5;
            height = 24;
            lenght = 23;
            width = 22;

            tags.Add("Barnes & Noble");
            tags.Add("Noble");

            images.Add("http://www.tdbaltic.ee/images/ds/BEU3088P-2.jpg");
            images.Add("http://www.tdbaltic.ee/images/ds/BEU3088P-3.jpg");
        }

        public string GetImportJsonString() {

            string 

            //        ""images"": [
            //            {{
            //                ""src"": ""http://www.tdbaltic.ee/images/ds/BEU3088P-2.jpg""
            //            }}
            //        ],
            //        ""tags"": [
            //            ""Barnes & Noble"",
            //            ""Big Air"",
            //            ""John's Fav""
            //        ],

            string retString = 
            @$"{{
                ""product"": {{
                    ""title"": {title},
                    ""body_html"": {body_html},
                    ""vendor"": {vendor},
                    ""product_type"": {product_type},
                    ""variants"": [
                        {{
                            ""price"": {price},
                            ""sku"": {sku},
                            ""weight"": {weight},
                            ""inventory_quantity"": {stock},
                            ""barcode"": {barcode}
                        }}
                    ],
                    ""metafields"": [
                        {{
                            ""key"": ""height"",
                            ""value"": {height},
                            ""value_type"": ""string"",
                            ""namespace"": ""dimensions""
                        }},
                        {{
                            ""key"": ""width"",
                            ""value"": {width},
                            ""value_type"": ""string"",
                            ""namespace"": ""dimensions""
                        }},
                        {{
                            ""key"": ""lenght"",
                            ""value"": {lenght},
                            ""value_type"": ""string"",
                            ""namespace"": ""dimensions""
                        }}
                    ],
                }}
            }}";

            return retString;
        }
    }
}
