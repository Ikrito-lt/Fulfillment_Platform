using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using static Ikrito_Fulfillment_Platform.Models.FullProduct;

namespace Ikrito_Fulfillment_Platform.Modules
{
    static class ProductModule
    {
        /// <summary>
        /// method for checking if product is out  of stock (to set its status) (if atleast one of the variants is in stock method return true)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool CheckIfProductOutOfStock(FullProduct p)
        {
            foreach (var variant in p.ProductVariants)
            {
                if (variant.Stock > 0)
                {
                    return true;
                }
            }
            return false;
        }

        //
        // Section with methods that are needed for product statuses
        //

        // method taht gets product tatus for DB using product SKU
        public static string GetProductStatus(string sku)
        {
            DataBaseInterface db = new();
            var whereQ = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            var productStatusResult = db.Table("Products").Where(whereQ).Get();
            string productStatus = productStatusResult[0]["Status"];

            return productStatus;
        }

        /// <summary>
        /// method that check if product is in database (returns true if exists)
        /// </summary>
        /// <param name="sku"></param>
        public static bool CheckIfExistsInDB(string sku)
        {
            DataBaseInterface db = new();

            string tablePrefix = sku.GetUntilOrEmpty();
            var whereGet = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            Dictionary<int, Dictionary<string, string>> result = db.Table($"_{tablePrefix}_Products").Where(whereGet).Get();
            if (result.Count != 0 && result[0].ContainsValue(sku))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// method that chages product status to new 
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="productType"></param>
        public static void MarkProductAsNew(string sku, string productType)
        {
            DataBaseInterface db = new();
            var InsertData = new Dictionary<string, string>
            {
                ["LastUpdateTime"] = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString(),
                ["ProductType_ID"] = productType,
                ["Status"] = ProductStatus.New,
                ["SKU"] = sku
            };
            db.Table("Products").Insert(InsertData);

        }

        /// <summary>
        /// method that changes product status to one passed to it (with conflict control)
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="newStatus"></param>
        /// <exception cref="Exception"></exception>
        public static void ChangeProductStatus(string sku, string newStatus, bool forceChangeNew)
        {
            //first we need to get product status and check if its "New"
            //if its "New" we cant change that unless method is called with force == true

            //setting up database query to get current status
            DataBaseInterface db = new();
            var whereQ = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            var productStatusResult = db.Table("Products").Where(whereQ).Get();
            string currentStatus = productStatusResult[0]["Status"];

            if (currentStatus == ProductStatus.New && forceChangeNew == false)
            {
                //exception to the rule we can change status (New -> Archived)
                if (newStatus == ProductStatus.Archived)
                {
                    var updateData = new Dictionary<string, string>
                    {
                        ["LastUpdateTime"] = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString(),
                        ["Status"] = newStatus
                    };
                    var whereUpdate = new Dictionary<string, Dictionary<string, string>>
                    {
                        ["SKU"] = new Dictionary<string, string>
                        {
                            ["="] = sku
                        }
                    };
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                }
                else if (forceChangeNew == false) {
                    throw new Exception($"Cant change product status {currentStatus} -> {newStatus}");
                }
            }
            else
            {
                //changing current status to newStatus
                var updateData = new Dictionary<string, string>
                {
                    ["LastUpdateTime"] = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString(),
                    ["Status"] = newStatus
                };
                var whereUpdate = new Dictionary<string, Dictionary<string, string>>
                {
                    ["SKU"] = new Dictionary<string, string>
                    {
                        ["="] = sku
                    }
                };
                db.Table("Products").Where(whereUpdate).Update(updateData);
            }
        }


        //
        // Section of methods that are responsible for managign individual products in database
        //

        // method for deleting product form database 
        public static void DeleteProduct(string sku)
        {
            string tablePrefix = sku.GetUntilOrEmpty();
            DataBaseInterface db = new DataBaseInterface();

            //deleting from Products table
            var whereDelete = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            db.Table("Products").Where(whereDelete).Delete();

        }

        //method that gets product from database using its SKU 
        public static FullProduct GetProduct(string sku)
        {
            FullProduct prod = new();
            string tablePrefix = sku.GetUntilOrEmpty();

            DataBaseInterface db = new();
            Dictionary<string, Dictionary<string, string>> whereCond;
            whereCond = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };

            //getting everything from _*_Products table
            var result = db.Table("_" + tablePrefix + "_Products").Where(whereCond).Get();
            foreach (var row in result.Values)
            {

                prod.TitleLT = row["Title"];
                prod.TitleLV = row["TitleLV"];
                prod.TitleEE = row["TitleEE"];
                prod.TitleRU = row["TitleRU"];

                prod.DescLT = row["Body"];
                prod.DescLV = row["BodyLV"];
                prod.DescEE = row["BodyEE"];
                prod.DescRU = row["BodyRU"];
                
                prod.Vendor = row["Vendor"];
                prod.ProductTypeID = row["ProductType_ID"];
                prod.SKU = row["SKU"];
                prod.Weight = double.Parse(row["Weight"]);
                prod.Height = int.Parse(row["Height"]);
                prod.Lenght = int.Parse(row["Lenght"]);
                prod.Width = int.Parse(row["Width"]);
                prod.AddedTimeStamp = row["AddedTimeStamp"];
                prod.ProductTypeVendor = row["ProductTypeVendor"];
            }

            //getting images faster
            whereCond = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            result = db.Table("_" + tablePrefix + "_Images").Where(whereCond).Get();
            foreach (var imgRow in result.Values)
            {
                string imageUrl = imgRow["ImgUrl"];
                prod.Images.Add(imageUrl);
            }

            //getting tags faster
            whereCond = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            result = db.Table("_" + tablePrefix + "_Tags").Where(whereCond).Get();
            foreach (var tagRow in result.Values)
            {
                string tag = tagRow["Tag"];

                prod.Tags.Add(tag);
            }

            //getting category dicplay value
            var catKVP = ProductCategoryModule.Instance.CategoryKVP;
            prod.ProductTypeDisplayVal = catKVP[prod.ProductTypeID];

            //getting product status
            whereCond = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            result = db.Table("Products").Where(whereCond).Get();
            prod.Status = result[0]["Status"];

            //getting product variants
            whereCond = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            result = db.Table("_" + tablePrefix + "_Variants").Where(whereCond).Get();
            foreach (var row in result.Values)
            {
                ProductVariant pVariant = new();
                pVariant.Barcode = row["Barcode"];
                pVariant.Stock = int.Parse(row["Stock"]);
                pVariant.Price = double.Parse(row["Price"]);
                pVariant.PriceVendor = double.Parse(row["PriceVendor"]);
                pVariant.VariantType = row["VariantData"];
                pVariant.VariantData = row["VariantData"];

                prod.ProductVariants.Add(pVariant);
            }

            //getting product attributtes
            whereCond = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            result = db.Table("_" + tablePrefix + "_Attributes").Where(whereCond).Get();
            foreach (var row in result.Values)
            {
                prod.ProductAttributtes.Add(row["Name"], row["Data"]);
            }

            return prod;
        }

        // method that adds new product to database (decides what table to add to using SKU prefix)
        public static void AddProductToDB(FullProduct p)
        {
            DataBaseInterface db = new();
            //checing if product exists in database if yes Change its status to New
            if (CheckIfExistsInDB(p.SKU))
            {
                //unarchaiving
                UpdateProductToDB(p, ProductStatus.New);
                return;
            }

            //getting categories KVP
            string tablePrefix = p.SKU.GetUntilOrEmpty();

            //adding product to Products table
            MarkProductAsNew(p.SKU, p.ProductTypeID);

            //inserting to *_Products table
            var InsertData = new Dictionary<string, string>
            {
                ["Title"] = p.TitleLT,
                ["TitleLV"] = p.TitleLV,
                ["TitleEE"] = p.TitleEE,
                ["TitleRU"] = p.TitleRU,

                ["Body"] = p.DescLT,
                ["BodyLV"] = p.DescLV,
                ["BodyEE"] = p.DescEE,
                ["BodyRU"] = p.DescRU,

                ["Vendor"] = p.Vendor,
                ["ProductType_ID"] = p.ProductTypeID,
                ["SKU"] = p.SKU,
                ["Weight"] = p.Weight.ToString(),
                ["Height"] = p.Height.ToString(),
                ["Lenght"] = p.Lenght.ToString(),
                ["Width"] = p.Width.ToString(),
                ["AddedTimeStamp"] = p.AddedTimeStamp,
                ["ProductTypeVendor"] = p.ProductTypeVendor,
                ["DeliveryTimeText"] = p.DeliveryTime
            };
            db.Table($"_{tablePrefix}_Products").Insert(InsertData);

            //add new Product images to DB
            foreach (var img in p.Images)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.SKU,
                    ["ImgUrl"] = img
                };
                db.Table($"_{tablePrefix}_Images").Insert(insertData);
            }

            //add new tags
            foreach (var tag in p.Tags)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.SKU,
                    ["Tag"] = tag
                };
                db.Table($"_{tablePrefix}_Tags").Insert(insertData);
            }

            //add new variants
            foreach (ProductVariant productVariant in p.ProductVariants)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.SKU,
                    ["Price"] = productVariant.Price.ToString(),
                    ["PriceVendor"] = productVariant.PriceVendor.ToString(),
                    ["Stock"] = productVariant.Stock.ToString(),
                    ["Barcode"] = productVariant.Barcode,
                    ["VariantType"] = productVariant.VariantType,
                    ["VariantData"] = productVariant.VariantData,
                    ["PermPrice"] = productVariant.PermPrice ? "1" : "0"
                };
                db.Table($"_{tablePrefix}_Variants").Insert(insertData);
            }

            //add new attributes
            foreach (var attrKVP in p.ProductAttributtes)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.SKU,
                    ["Name"] = SQLUtil.SQLSafeString(attrKVP.Key),
                    ["Data"] = SQLUtil.SQLSafeString(attrKVP.Value)
                };
                db.Table($"_{tablePrefix}_Attributes").Insert(insertData);
            }
        }

        /// <summary>
        /// method that updates data of existing product in the database, and changes its status (used in product edit page)
        /// </summary>
        /// <param name="p"></param>
        /// <param name="status"></param>
        public static void UpdateProductToDB(FullProduct p, string status)
        {
            DataBaseInterface db = new();
            string tablePrefix = p.SKU.GetUntilOrEmpty();

            //updating *_Products table
            var updateData = new Dictionary<string, string>
            {
                ["Title"] = p.TitleLT,
                ["TitleLV"] = p.TitleLV,
                ["TitleEE"] = p.TitleEE,
                ["TitleRU"] = p.TitleRU,

                ["Body"] = p.DescLT,
                ["BodyLV"] = p.DescLV,
                ["BodyEE"] = p.DescEE,
                ["BodyRU"] = p.DescRU,

                ["Vendor"] = p.Vendor,
                ["ProductType_ID"] = p.ProductTypeID,
                ["Weight"] = p.Weight.ToString(),
                ["Height"] = p.Height.ToString(),
                ["Lenght"] = p.Lenght.ToString(),
                ["Width"] = p.Width.ToString(),
                ["AddedTimeStamp"] = p.AddedTimeStamp,
                ["ProductTypeVendor"] = p.ProductTypeVendor

            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = p.SKU
                }
            };
            db.Table($"_{tablePrefix}_Products").Where(whereUpdate).Update(updateData);

            //load all images of the product
            var whereQ = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = p.SKU
                }
            };
            var oldImages = db.Table($"_{tablePrefix}_Images").Where(whereQ).Get();

            //delete all images
            foreach (var img in oldImages.Values)
            {
                var whereDelete = new Dictionary<string, Dictionary<string, string>>
                {
                    ["ID"] = new Dictionary<string, string>
                    {
                        ["="] = img["ID"]
                    }
                };
                db.Table($"_{tablePrefix}_Images").Where(whereDelete).Delete();
            }

            //add new images
            foreach (var img in p.Images)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.SKU,
                    ["ImgUrl"] = img
                };
                db.Table($"_{tablePrefix}_Images").Insert(insertData);
            }

            //load all tags of the product
            var oldTags = db.Table($"_{tablePrefix}_Tags").Where(whereQ).Get();

            //delete all tags
            foreach (var tag in oldTags.Values)
            {
                var whereDelete = new Dictionary<string, Dictionary<string, string>>
                {
                    ["ID"] = new Dictionary<string, string>
                    {
                        ["="] = tag["ID"]
                    }
                };
                db.Table($"_{tablePrefix}_Tags").Where(whereDelete).Delete();
            }

            //add new tags
            foreach (var tag in p.Tags)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.SKU,
                    ["Tag"] = tag
                };
                db.Table($"_{tablePrefix}_Tags").Insert(insertData);
            }

            //load all Variants of the product
            var oldVariants = db.Table($"_{tablePrefix}_Variants").Where(whereQ).Get();

            //delete all Variants
            foreach (var row in oldVariants.Values)
            {
                var whereDelete = new Dictionary<string, Dictionary<string, string>>
                {
                    ["ID"] = new Dictionary<string, string>
                    {
                        ["="] = row["ID"]
                    }
                };
                db.Table($"_{tablePrefix}_Variants").Where(whereDelete).Delete();
            }

            //update variants
            foreach (ProductVariant productVariant in p.ProductVariants)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.SKU,
                    ["Price"] = productVariant.Price.ToString(),
                    ["PriceVendor"] = productVariant.PriceVendor.ToString(),
                    ["Stock"] = productVariant.Stock.ToString(),
                    ["Barcode"] = productVariant.Barcode,
                    ["VariantType"] = productVariant.VariantType,
                    ["VariantData"] = productVariant.VariantData,
                    ["PermPrice"] = productVariant.PermPrice ? "1" : "0"
                };
                db.Table($"_{tablePrefix}_Variants").Insert(insertData);
            }

            //load all attributtes of the product
            var oldAttributes = db.Table($"_{tablePrefix}_Attributes").Where(whereQ).Get();

            //delete all attributes
            foreach (var row in oldAttributes.Values)
            {
                var whereDelete = new Dictionary<string, Dictionary<string, string>>
                {
                    ["ID"] = new Dictionary<string, string>
                    {
                        ["="] = row["ID"]
                    }
                };
                db.Table($"_{tablePrefix}_Attributes").Where(whereDelete).Delete();
            }

            //add new attributes
            foreach (var attrKVP in p.ProductAttributtes)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.SKU,
                    ["Name"] = attrKVP.Key,
                    ["Data"] = attrKVP.Value
                };
                db.Table($"_{tablePrefix}_Attributes").Insert(insertData);
            }

            //changing productTypeID in Products table
            updateData = new Dictionary<string, string>
            {
                ["ProductType_ID"] = p.ProductTypeID
            };
            whereUpdate = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = p.SKU
                }
            };
            db.Table($"Products").Where(whereUpdate).Update(updateData);

            //changing product status
            try
            {
                ChangeProductStatus(p.SKU, status, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        //
        // Section for getting product lists from database
        //

        //method gets list of TDB products
        public static Dictionary<string, FullProduct> GetVendorProducts(string TablePrefix)
        {
            Dictionary<string, FullProduct> productsKVP = new();

            //getting main product info
            DataBaseInterface db = new();
            var result = db.Table($"_{TablePrefix}_Products").Get();
            foreach (var prod in result.Values)
            {

                FullProduct NewProduct = new();
                NewProduct.TitleLT = prod["Title"];
                NewProduct.TitleLV = prod["TitleLV"];
                NewProduct.TitleEE = prod["TitleEE"];
                NewProduct.TitleRU = prod["TitleRU"];

                NewProduct.DescLT = prod["Body"];
                NewProduct.DescLV = prod["BodyLV"];
                NewProduct.DescEE = prod["BodyEE"];
                NewProduct.DescRU = prod["BodyRU"];

                NewProduct.Vendor = prod["Vendor"];
                NewProduct.ProductTypeID = prod["ProductType_ID"];
                NewProduct.SKU = prod["SKU"];
                NewProduct.Weight = double.Parse(prod["Weight"]);
                NewProduct.Height = int.Parse(prod["Height"]);
                NewProduct.Lenght = int.Parse(prod["Lenght"]);
                NewProduct.Width = int.Parse(prod["Width"]);

                NewProduct.AddedTimeStamp = prod["AddedTimeStamp"];
                NewProduct.DeliveryTime = prod["DeliveryTimeText"];
                NewProduct.ProductTypeVendor = prod["ProductTypeVendor"];

                productsKVP.Add(prod["SKU"], NewProduct);
            }

            //getting images faster
            result = db.Table($"_{TablePrefix}_Images").Get();
            foreach (var imgRow in result.Values)
            {

                string sku = imgRow["SKU"];
                string imageUrl = imgRow["ImgUrl"];

                productsKVP[sku].Images.Add(imageUrl);
            }

            //getting tags faster
            result = db.Table($"_{TablePrefix}_Tags").Get();
            foreach (var tagRow in result.Values)
            {

                string sku = tagRow["SKU"];
                string tag = tagRow["Tag"];

                productsKVP[sku].Tags.Add(tag);
            }

            //getting variants faster
            result = db.Table($"_{TablePrefix}_Variants").Get();
            foreach (var row in result.Values)
            {
                string sku = row["SKU"];
                ProductVariant variant = new();
                variant.VariantDBID = int.Parse(row["ID"]);
                variant.Price = double.Parse(row["Price"]);
                variant.PriceVendor = double.Parse(row["PriceVendor"]);
                variant.Stock = int.Parse(row["Stock"]);
                variant.Barcode = row["Barcode"];
                variant.VariantType = row["VariantType"];
                variant.VariantData = row["VariantData"];
                variant.PermPrice = row["PermPrice"] == "False" ? false : true;

                productsKVP[sku].ProductVariants.Add(variant);
            }

            //getting attributes faster
            result = db.Table($"_{TablePrefix}_Attributes").Get();
            foreach (var row in result.Values)
            {
                string sku = row["SKU"];
                string name = row["Name"];
                string data = row["Data"];

                productsKVP[sku].ProductAttributtes.Add(name, data);
            }

            //getting products statuses
            result = db.ExecuteTextQuery($"SELECT SKU, Status FROM Products WHERE SKU LIKE \"{TablePrefix}%\";");
            foreach (var statusRow in result.Values)
            {
                string sku = statusRow["SKU"];
                string status = statusRow["Status"];

                productsKVP[sku].Status = status;
            }
            return productsKVP;
        }

        //todo: this is slow
        //method gets list of all Products in database
        public static Dictionary<string,FullProduct> GetAllProducts()
        {
            Dictionary<string, FullProduct> p = new();

            //Dictionary<string, FullProduct> TDBproducts = GetVendorProducts("TDB");
            //p.AddRange(TDBproducts);

            Dictionary<string, FullProduct> KGproducts = GetVendorProducts("KG");
            p.AddRange(KGproducts);

            Dictionary<string, FullProduct> PDproducts = GetVendorProducts("PD");
            p.AddRange(PDproducts);

            Dictionary<string, FullProduct> BFproducts = GetVendorProducts("BF");
            p.AddRange(BFproducts);

            return p;
        }
    }
}