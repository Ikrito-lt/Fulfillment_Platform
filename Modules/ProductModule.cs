using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using static Ikrito_Fulfillment_Platform.Models.Product;

namespace Ikrito_Fulfillment_Platform.Modules
{
    static class ProductModule
    {

        //
        // section for product category manipulation
        //

        /// <summary>
        /// changes product category in the database
        /// </summary> 
        /// <param name="sku"></param>
        /// <param name="newCategoryID"></param>
        public static void ChangeProductCategory(string sku, string newCategoryID)
        {
            DataBaseInterface db = new();
            string tablePrefix = sku.GetUntilOrEmpty();

            //updating *_Products table
            var updateData = new Dictionary<string, string>
            {
                ["ProductType"] = newCategoryID
            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            db.Table($"_{tablePrefix}_Products").Where(whereUpdate).Update(updateData);
        }

        //gets Categories KVP from database
        public static Dictionary<string, string> GetCategoriesDictionary()
        {
            //getting category KVP from database
            Dictionary<string, string> categoriesKVP = new();
            DataBaseInterface db = new();

            var result = db.Table("ProductTypes").Get("ID, ProductType");
            foreach (var cat in result.Values)
            {

                var id = cat["ID"];
                var type = cat["ProductType"];

                categoriesKVP.Add(id, type);
            }
            return categoriesKVP;
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
        /// method that check if product is in database
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

        //method that changes product status to one passed to it (with conflict control)
        public static void ChangeProductStatus(string sku, string status)
        {

            //first we need to get product status and check if its "New"
            //if its "New" we cant change that
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

            var updateData = new Dictionary<string, string>
            {
                ["LastUpdateTime"] = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString(),
                ["Status"] = status
            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };

            if (productStatus == ProductStatus.New)
            {
                if (status == ProductStatus.Ok)
                {
                    updateData["Status"] = ProductStatus.Ok;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else if (status == ProductStatus.WaitingShopSync)
                {
                    //edge case
                    updateData["Status"] = ProductStatus.New;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else if (status == ProductStatus.NeedsArchiving)
                {
                    //edge case
                    DeleteProduct(sku);
                }
                else
                {
                    throw new Exception($"cant change product status {productStatus} -> {status}");
                }

            }
            else if (productStatus == ProductStatus.Ok)
            {

                if (status == ProductStatus.NeedsArchiving)
                {
                    updateData["Status"] = ProductStatus.NeedsArchiving;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else if (status == ProductStatus.WaitingShopSync)
                {
                    updateData["Status"] = ProductStatus.WaitingShopSync;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else if (status == ProductStatus.NeedsArchiving)
                {
                    updateData["Status"] = ProductStatus.NeedsArchiving;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else
                {
                    throw new Exception($"cant change product status {productStatus} -> {status}");
                }

            }
            else if (productStatus == ProductStatus.NeedsArchiving)
            {

                if (status == ProductStatus.Archived)
                {
                    updateData["Status"] = ProductStatus.Archived;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else if (status == ProductStatus.NeedsUnArchiving)
                {
                    //edge case
                    updateData["Status"] = ProductStatus.WaitingShopSync;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else if (status == ProductStatus.NeedsArchiving)
                {
                    updateData["Status"] = ProductStatus.NeedsArchiving;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else if (status == ProductStatus.WaitingShopSync)
                {
                    updateData["Status"] = ProductStatus.WaitingShopSync;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else
                {
                    throw new Exception($"cant change product status {productStatus} -> {status}");
                }

            }
            else if (productStatus == ProductStatus.Archived)
            {

                if (status == ProductStatus.NeedsUnArchiving)
                {
                    updateData["Status"] = ProductStatus.NeedsUnArchiving;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else if (status == ProductStatus.WaitingShopSync)
                {
                    //edge case
                    updateData["Status"] = ProductStatus.NeedsUnArchiving;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else if (status == ProductStatus.NeedsArchiving)
                {
                    //edge case
                    updateData["Status"] = ProductStatus.Archived;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else
                {
                    throw new Exception($"cant change product status {productStatus} -> {status}");
                }

            }
            else if (productStatus == ProductStatus.WaitingShopSync)
            {

                if (status == ProductStatus.Ok)
                {
                    updateData["Status"] = ProductStatus.Ok;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                }
                else if (status == ProductStatus.NeedsArchiving)
                {
                    updateData["Status"] = ProductStatus.NeedsArchiving;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                }
                else if (status == ProductStatus.WaitingShopSync)
                {
                    updateData["Status"] = ProductStatus.WaitingShopSync;
                    db.Table("Products").Where(whereUpdate).Update(updateData);
                }
                else
                {
                    throw new Exception($"cant change product status {productStatus} -> {status}");
                }

            }
            else if (productStatus == ProductStatus.NeedsUnArchiving)
            {

                if (status == ProductStatus.WaitingShopSync)
                {
                    updateData["Status"] = ProductStatus.WaitingShopSync;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else if (status == ProductStatus.NeedsArchiving)
                {
                    updateData["Status"] = ProductStatus.NeedsArchiving;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else if (status == ProductStatus.WaitingShopSync)
                {
                    updateData["Status"] = ProductStatus.NeedsUnArchiving;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else if (status == ProductStatus.Ok)
                {
                    updateData["Status"] = ProductStatus.NeedsUnArchiving;
                    db.Table("Products").Where(whereUpdate).Update(updateData);

                }
                else
                {
                    throw new Exception($"cant change product status {productStatus} -> {status}");
                }

            }
            else
            {
                throw new Exception($"cant change product status {productStatus} -> {status}");
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

        // method that gets product from database using its SKU
        public static Product GetProduct(string sku)
        {
            Product prod = new();
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

                prod.title = row["Title"];
                prod.body_html = row["Body"];
                prod.vendor = row["Vendor"];
                prod.productTypeID = row["ProductType"];
                prod.sku = row["SKU"];
                prod.weight = double.Parse(row["Weight"]);
                prod.height = int.Parse(row["Height"]);
                prod.lenght = int.Parse(row["Lenght"]);
                prod.width = int.Parse(row["Width"]);
                prod.addedTimeStamp = row["AddedTimeStamp"];
                prod.productTypeVendor = row["ProductTypeVendor"];
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
                prod.images.Add(imageUrl);
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

                prod.tags.Add(tag);
            }

            //getting category dicplay value
            var catKVP = GetCategoriesDictionary();
            prod.ProductTypeDisplayVal = catKVP[prod.productTypeID];

            //getting product status
            whereCond = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            result = db.Table("Products").Where(whereCond).Get();
            prod.status = result[0]["Status"];

            //getting product variants
            whereCond = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            result = db.Table("_" + tablePrefix + "Variants").Where(whereCond).Get();
            foreach (var row in result.Values)
            {
                ProductVariant pVariant = new();
                pVariant.barcode = row["Barcode"];
                pVariant.stock = int.Parse(row["Stock"]);
                pVariant.price = double.Parse(row["Price"]);
                pVariant.vendor_price = double.Parse(row["PriceVendor"]);
                pVariant.VariantType = row["VariantData"];
                pVariant.VariantData = row["VariantData"];

                prod.productVariants.Add(pVariant);
            }

            //getting product attributtes
            whereCond = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            result = db.Table("_" + tablePrefix + "Attributes").Where(whereCond).Get();
            foreach (var row in result.Values)
            {
                prod.productAttributtes.Add(row["Name"], row["Data"]);
            }

            return prod;
        }

        // method that adds new product to database (decides what table to add to using SKU prefix)
        public static void AddProductToDB(Product p)
        {
            DataBaseInterface db = new();
            //checing if product exists in database if yes unarchaving it
            if (CheckIfExistsInDB(p.sku))
            {
                //unarchaiving
                UpdateProductToDB(p, ProductStatus.NeedsUnArchiving);
                return;
            }

            //getting categories KVP
            string tablePrefix = p.sku.GetUntilOrEmpty();

            //adding product to Products table
            MarkProductAsNew(p.sku, p.productTypeID);

            //inserting to *_Products table
            var InsertData = new Dictionary<string, string>
            {
                ["Title"] = p.title,
                ["Body"] = p.body_html,
                ["Vendor"] = p.vendor,
                ["ProductType_ID"] = p.productTypeID,
                ["SKU"] = p.sku,
                ["Weight"] = p.weight.ToString(),
                ["Height"] = p.height.ToString(),
                ["Lenght"] = p.lenght.ToString(),
                ["Width"] = p.width.ToString(),
                ["AddedTimeStamp"] = p.addedTimeStamp,
                ["ProductTypeVendor"] = p.productTypeVendor,
                ["DeliveryTimeText"] = p.deliveryTime
            };
            db.Table($"_{tablePrefix}_Products").Insert(InsertData);

            //add new Product images to DB
            foreach (var img in p.images)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.sku,
                    ["ImgUrl"] = img
                };
                db.Table($"_{tablePrefix}_Images").Insert(insertData);
            }

            //add new tags
            foreach (var tag in p.tags)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.sku,
                    ["Tag"] = tag
                };
                db.Table($"_{tablePrefix}_Tags").Insert(insertData);
            }

            //add new variants
            foreach (ProductVariant productVariant in p.productVariants)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.sku,
                    ["Price"] = productVariant.price.ToString(),
                    ["PriceVendor"] = productVariant.vendor_price.ToString(),
                    ["Stock"] = productVariant.stock.ToString(),
                    ["Barcode"] = productVariant.barcode,
                    ["VariantType"] = productVariant.VariantType,
                    ["VariantData"] = productVariant.VariantData
                };
                db.Table($"_{tablePrefix}_Variants").Insert(insertData);
            }

            //add new attributes
            foreach (var attrKVP in p.productAttributtes)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.sku,
                    ["Name"] = attrKVP.Key,
                    ["Data"] = attrKVP.Value
                };
                db.Table($"_{tablePrefix}_Attributes").Insert(insertData);
            }
        }

        // method that updates data of existing product in the database, and the changes its status
        public static void UpdateProductToDB(Product p, string status)
        {
            DataBaseInterface db = new();
            string tablePrefix = p.sku.GetUntilOrEmpty();

            //updating *_Products table
            var updateData = new Dictionary<string, string>
            {
                ["Title"] = p.title,
                ["Body"] = p.body_html,
                ["Vendor"] = p.vendor,
                ["ProductType"] = p.productTypeID,
                ["Weight"] = p.weight.ToString(),
                ["Height"] = p.height.ToString(),
                ["Lenght"] = p.lenght.ToString(),
                ["Width"] = p.width.ToString(),
                ["AddedTimeStamp"] = p.addedTimeStamp,
                ["ProductTypeVendor"] = p.productTypeVendor

            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = p.sku
                }
            };
            db.Table($"_{tablePrefix}_Products").Where(whereUpdate).Update(updateData);

            //load all images of the product
            var whereQ = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = p.sku
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
            foreach (var img in p.images)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.sku,
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
            foreach (var tag in p.tags)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.sku,
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
            foreach (ProductVariant productVariant in p.productVariants)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.sku,
                    ["Price"] = productVariant.price.ToString(),
                    ["PriceVendor"] = productVariant.vendor_price.ToString(),
                    ["Stock"] = productVariant.stock.ToString(),
                    ["Barcode"] = productVariant.barcode,
                    ["VariantType"] = productVariant.VariantType,
                    ["VariantData"] = productVariant.VariantData
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
                db.Table($"_{tablePrefix}_Variants").Where(whereDelete).Delete();
            }

            //add new attributes
            foreach (var attrKVP in p.productAttributtes)
            {
                var insertData = new Dictionary<string, string>
                {
                    ["SKU"] = p.sku,
                    ["Name"] = attrKVP.Key,
                    ["Data"] = attrKVP.Value
                };
                db.Table($"_{tablePrefix}_Variants").Insert(insertData);
            }

            try
            {
                ChangeProductStatus(p.sku, status);
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
        public static List<Product> GetVendorProducts(string TablePrefix)
        {
            List<Product> products = new();

            //getting main product info
            DataBaseInterface db = new();
            var result = db.Table($"_{TablePrefix}_Products").Get();
            foreach (var prod in result.Values)
            {

                Product NewProduct = new();
                NewProduct.title = prod["Title"];
                NewProduct.body_html = prod["Body"];
                NewProduct.vendor = prod["Vendor"];
                NewProduct.productTypeID = prod["ProductType_ID"];
                NewProduct.sku = prod["SKU"];
                NewProduct.weight = double.Parse(prod["Weight"]);
                NewProduct.height = int.Parse(prod["Height"]);
                NewProduct.lenght = int.Parse(prod["Lenght"]);
                NewProduct.width = int.Parse(prod["Width"]);

                NewProduct.addedTimeStamp = prod["AddedTimeStamp"];
                NewProduct.productTypeVendor = prod["ProductTypeVendor"];

                products.Add(NewProduct);
            }

            //getting images faster
            result = db.Table($"_{TablePrefix}_Images").Get();
            foreach (var imgRow in result.Values)
            {

                string sku = imgRow["SKU"];
                string imageUrl = imgRow["ImgUrl"];

                products.Find(x => x.sku == sku).images.Add(imageUrl);
            }

            //getting tags faster
            result = db.Table($"_{TablePrefix}_Tags").Get();
            foreach (var tagRow in result.Values)
            {

                string sku = tagRow["SKU"];
                string tag = tagRow["Tag"];

                products.Find(x => x.sku == sku).tags.Add(tag);
            }

            //getting variants faster
            result = db.Table($"_{TablePrefix}_Variants").Get();
            foreach (var row in result.Values)
            {
                string sku = row["SKU"];
                ProductVariant variant = new();
                variant.variantDBID = int.Parse(row["ID"]);
                variant.price = double.Parse(row["Price"]);
                variant.vendor_price = double.Parse(row["PriceVendor"]);
                variant.stock = int.Parse(row["Stock"]);
                variant.barcode = row["Barcode"];
                variant.VariantType = row["VariantType"];
                variant.VariantData = row["VariantData"];

                products.Find(x => x.sku == sku).productVariants.Add(variant);
            }

            //getting attributes faster
            result = db.Table($"_{TablePrefix}_Attributes").Get();
            foreach (var row in result.Values)
            {
                string sku = row["SKU"];
                string name = row["Name"];
                string data = row["Data"];

                products.Find(x => x.sku == sku).productAttributtes.Add(name, data);
            }

            return products;
        }

        //method gets list of all Products in database
        public static List<Product> GetAllProducts()
        {
            List<Product> p = new();

            List<Product> TDBproducts = GetVendorProducts("TDB");
            p.AddRange(TDBproducts);

            List<Product> KGproducts = GetVendorProducts("KG");
            p.AddRange(KGproducts);

            List<Product> PDproducts = GetVendorProducts("PD");
            p.AddRange(PDproducts);

            //getting product statuses faster
            DataBaseInterface db = new();
            var result = db.Table("Products").Get();
            foreach (var statusRow in result.Values)
            {

                string sku = statusRow["SKU"];
                string status = statusRow["Status"];

                if (p.Exists(x => x.sku == sku))
                {
                    p.Find(x => x.sku == sku).status = status;
                }
            }
            return p;
        }
    }
}