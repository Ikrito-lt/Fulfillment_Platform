using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules.Supplier.BeFancy;
using Ikrito_Fulfillment_Platform.Modules.Supplier.KotrynaGroup;
using Ikrito_Fulfillment_Platform.Modules.Supplier.Pretendentas;
using Ikrito_Fulfillment_Platform.Modules.Supplier.TDBaltic;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using static Ikrito_Fulfillment_Platform.Models.Product;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier
{
    static class UploadSupplierProducts
    {

        /// <summary>
        /// gets supplier products using their TablePrefix
        /// </summary>
        /// <param name="TablePrefix"></param>
        /// <returns></returns>
        private static List<Product> GetSupplierProductList(string TablePrefix) {
            List<Product> pList = new();

            //todo: redo
            if (TablePrefix == "TDB")
            {
                pList.AddRange(TDBModule.ProductList);
            }
            else if (TablePrefix == "KG")
            {
                pList.AddRange(KGModule.ProductList);
            }
            else if (TablePrefix == "PD")
            {
                pList.AddRange(PDModule.ProductList);
            }
            else if (TablePrefix == "BF")
            {
                pList.AddRange(BFModule.ProductList);
            }
            return pList;
        }



        /// <summary>
        /// Updates product and then sends products that dont exist to addNewProduct method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void UpdateProducts(string TablePrefix, object sender = null, DoWorkEventArgs e = null)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            //getting DB Products
            Dictionary<string, Product> DBProducts = new Dictionary<string, Product>();
            foreach (Product DBProduct in ProductModule.GetVendorProducts(TablePrefix)) {
                DBProducts.Add(DBProduct.sku, DBProduct);
            }

            //getting API products
            Dictionary<string, Product> APIProducts = new Dictionary<string, Product>();
            foreach (Product APIProduct in GetSupplierProductList(TablePrefix)) {
                APIProducts.Add(APIProduct.sku, APIProduct);
            }

            //sorting
            List<string> ArchiveProductSKUs = DBProducts.Keys.Except(APIProducts.Keys).ToList();
            List<string> UpdateProductSKUs = DBProducts.Keys.Intersect(APIProducts.Keys).ToList();
            List<string> NewProductSKUs = APIProducts.Keys.Except(DBProducts.Keys).ToList();

            List<Product> ArchiveProducts = DBProducts.Values.Where(x => ArchiveProductSKUs.Contains(x.sku)).ToList();
            List<Product> UpdateProducts = APIProducts.Values.Where(x => UpdateProductSKUs.Contains(x.sku)).ToList();
            List<Product> NewProducts = APIProducts.Values.Where(x => NewProductSKUs.Contains(x.sku)).ToList();




            ////sorting
            //List<KeyValuePair<string, Product>> ArchiveProducts = DBProducts.Where(p1 => APIProducts.All(p2 => p2.Key != p1.Key)).ToList();
            //List<KeyValuePair<string, Product>> NewProducts = APIProducts.Where(p1 => DBProducts.All(p2 => p2.Key != p1.Key)).ToList();
            //List<KeyValuePair<string, Product>> UpdateProducts = APIProducts.Where(p1 => NewProducts.All(p2 => p2.Key != p1.Key)).ToList();



            //Dictionary<string, Product> ArchiveProducts = DBProducts.Where(p1 => APIProducts.All(p2 => p2.sku != p1.sku)).ToList();
            //Dictionary<string, Product> NewProducts = APIProducts.Where(p1 => DBProducts.All(p2 => p2.sku != p1.sku)).ToList();
            //Dictionary<string, Product> UpdateProducts = APIProducts.Where(p1 => NewProducts.All(p2 => p2.sku != p1.sku)).ToList();

            //for remorting progress in listboxes
            List<ProductChangeRecord> appliedChanges = new();          //for updates
            List<ProductChangeRecord> newChanges = new();              //for new products
            List<ProductChangeRecord> archivedChanges = new();         //for archived Products                     

            //todo: i need to see what products in updateProducts list i actually need to update to make this number exact
            //for progress reporting
            int archiveProductsLenght = ArchiveProducts.Count();
            int newProductsLenght = NewProducts.Count();
            int updateProductsLenght = UpdateProducts.Count();
            int allProductsActionCount = archiveProductsLenght + updateProductsLenght + newProductsLenght;
            int allProductsActionDone = 0;

            //archiving products
            foreach (var (archiveProduct, index) in ArchiveProducts.LoopIndex())
            {
                try
                {
                    ProductModule.ChangeProductStatus(archiveProduct.sku, ProductStatus.NeedsArchiving);
                    ProductVariant firstVariant = archiveProduct.productVariants.First();

                    ProductChangeRecord archiveChange = new ProductChangeRecord {
                        SKU = archiveProduct.sku,
                        PriceVendor = firstVariant.vendor_price.ToString(),
                        Price = firstVariant.price.ToString(),
                        Stock = firstVariant.price.ToString(),
                        Barcode = firstVariant.barcode.ToString(),
                        Vendor = archiveProduct.vendor,
                        VendorProductType = archiveProduct.productTypeVendor,
                        ProductType = archiveProduct.ProductTypeDisplayVal,
                        Status = archiveProduct.status,
                        VariantData = $"{firstVariant.VariantType} - {firstVariant.VariantData}",
                        VariantCount = archiveProduct.productVariants.Count(),
                        ChangesMade = new()
                    };
                    archiveChange.ChangesMade.Add("Archived");
                    archivedChanges.Add(archiveChange);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.",
                        "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //reporting progress
                allProductsActionDone += 1;
                int promiles = (allProductsActionDone * 1000) / allProductsActionCount;
                worker.ReportProgress(promiles, $"Archiving {TablePrefix} Products ({index + 1}/{archiveProductsLenght}) - Total Actions ({allProductsActionDone}/{allProductsActionCount})");
            }

            // adding new Products
            foreach (var (newProduct, index) in NewProducts.LoopIndex())
            {
                ProductModule.AddProductToDB(newProduct);

                ProductVariant firstVariant = newProduct.productVariants.First();
                ProductChangeRecord newChange = new ProductChangeRecord
                {
                    SKU = newProduct.sku,
                    PriceVendor = firstVariant.vendor_price.ToString(),
                    Price = firstVariant.price.ToString(),
                    Stock = firstVariant.price.ToString(),
                    Barcode = firstVariant.barcode.ToString(),
                    Vendor = newProduct.vendor,
                    VendorProductType = newProduct.productTypeVendor,
                    ProductType = newProduct.ProductTypeDisplayVal,
                    Status = newProduct.status,
                    VariantData = $"{firstVariant.VariantType} - {firstVariant.VariantData}",
                    VariantCount = newProduct.productVariants.Count(),
                    ChangesMade = new()
                };
                newChange.ChangesMade.Add("Added to Database");
                newChanges.Add(newChange);

                //reporting progress
                allProductsActionDone += 1;
                int promiles = (allProductsActionDone * 1000) / allProductsActionCount;
                worker.ReportProgress(promiles, $"Adding New {TablePrefix} Products ({index + 1}/{newProductsLenght}) - Total Actions ({allProductsActionDone}/{allProductsActionCount})");
            }

            //updating products
            DataBaseInterface db = new();
            foreach (var (updateProduct, index) in UpdateProducts.LoopIndex())
            {
                Product oldProduct = DBProducts[updateProduct.sku];

                foreach (ProductVariant pVariant in updateProduct.productVariants) {
                    ProductVariant dbVariant = oldProduct.productVariants.Where(x => x.barcode == pVariant.barcode).FirstOrDefault();

                    ProductChangeRecord updateChange = new ProductChangeRecord
                    {
                        SKU = updateProduct.sku,
                        Barcode = pVariant.barcode.ToString(),
                        Vendor = updateProduct.vendor,
                        VendorProductType = updateProduct.productTypeVendor,
                        ProductType = updateProduct.ProductTypeDisplayVal,
                        Status = updateProduct.status,
                        VariantCount = updateProduct.productVariants.Count(),
                        ChangesMade = new()
                    }; 

                    if (dbVariant != null)
                    {
                        //update viariant (check if there are changes to vendor price or stock)
                        if (!dbVariant.isSame(pVariant))
                        {
                            //change variant (check if price is permament too)
                            if (dbVariant.PermPrice)
                            {
                                //price is permament
                                var variantUpdateData = new Dictionary<string, string>
                                {
                                    ["Price"] = dbVariant.price.ToString(),
                                    ["PriceVendor"] = pVariant.vendor_price.ToString(),
                                    ["Stock"] = pVariant.stock.ToString(),
                                    ["VariantType"] = pVariant.VariantType,
                                    ["VariantData"] = pVariant.VariantData,
                                };
                                var variantWhereUpdate = new Dictionary<string, Dictionary<string, string>>
                                {
                                    ["ID"] = new Dictionary<string, string>
                                    {
                                        ["="] = dbVariant.variantDBID.ToString()
                                    }
                                };
                                db.Table($"_{TablePrefix}_Variants").Where(variantWhereUpdate).Update(variantUpdateData);

                                updateChange.Price = dbVariant.price.ToString();
                                updateChange.PriceVendor = pVariant.vendor_price.ToString();
                                updateChange.Stock = pVariant.stock.ToString();
                                updateChange.VariantData = $"{pVariant.VariantType} - {pVariant.VariantData}";
                                updateChange.ChangesMade.Add($"Changed stock:\t\t{dbVariant.stock} -> {pVariant.stock}");
                                updateChange.ChangesMade.Add($"Price is permament check profit!");
                                updateChange.ChangesMade.Add($"Changed vendor price:\t\t{dbVariant.vendor_price} -> {pVariant.vendor_price}");
                            }
                            else {
                                //price isnt permament
                                var variantUpdateData = new Dictionary<string, string>
                                {
                                    ["Price"] = pVariant.price.ToString(),
                                    ["PriceVendor"] = pVariant.vendor_price.ToString(),
                                    ["Stock"] = pVariant.stock.ToString(),
                                    ["VariantType"] = pVariant.VariantType,
                                    ["VariantData"] = pVariant.VariantData,
                                };
                                var variantWhereUpdate = new Dictionary<string, Dictionary<string, string>>
                                {
                                    ["ID"] = new Dictionary<string, string>
                                    {
                                        ["="] = dbVariant.variantDBID.ToString()
                                    }
                                };
                                db.Table($"_{TablePrefix}_Variants").Where(variantWhereUpdate).Update(variantUpdateData);

                                updateChange.Price = dbVariant.price.ToString();
                                updateChange.PriceVendor = pVariant.vendor_price.ToString();
                                updateChange.Stock = pVariant.stock.ToString();
                                updateChange.VariantData = $"{pVariant.VariantType} - {pVariant.VariantData}";
                                updateChange.ChangesMade.Add($"Changed stock:\t\t{dbVariant.stock} -> {pVariant.stock}");
                                updateChange.ChangesMade.Add($"Changed price:\t\t{dbVariant.price} -> {pVariant.price}");
                                updateChange.ChangesMade.Add($"Changed vendor price:\t\t{dbVariant.vendor_price} -> {pVariant.vendor_price}");
                            }
                        }
                    }
                    else {
                        //check if there are any variants with out barcode in this product and delete them
                        var barcodeLessVariants = oldProduct.productVariants.Where(x => string.IsNullOrEmpty(x.barcode));
                        foreach (ProductVariant barcodeLessVariant in barcodeLessVariants) {
                            var variantWhereDelete = new Dictionary<string, Dictionary<string, string>>
                            {
                                ["ID"] = new Dictionary<string, string>
                                {
                                    ["="] = barcodeLessVariant.variantDBID.ToString()
                                }
                            };
                            db.Table($"_{TablePrefix}_Variants").Where(variantWhereDelete).Delete();
                        }

                        // add new variant
                        var insertData = new Dictionary<string, string>
                        {
                            ["SKU"] = updateProduct.sku,
                            ["Price"] = pVariant.price.ToString(),
                            ["PriceVendor"] = pVariant.vendor_price.ToString(),
                            ["Stock"] = pVariant.stock.ToString(),
                            ["Barcode"] = pVariant.barcode,
                            ["VariantType"] = pVariant.VariantType,
                            ["VariantData"] = pVariant.VariantData,
                            ["PermPrice"] = pVariant.PermPrice ? "1" : "0"
                        };
                        db.Table($"_{TablePrefix}_Variants").Insert(insertData);

                        updateChange.Price = pVariant.price.ToString();
                        updateChange.PriceVendor = pVariant.vendor_price.ToString();
                        updateChange.Stock = pVariant.stock.ToString();
                        updateChange.VariantData = $"{pVariant.VariantType} - {pVariant.VariantData}";
                        updateChange.ChangesMade.Add($"Added Variant + Deleted Variants Without Barcode");
                    }

                    if (updateChange.ChangesMade.Count > 0)
                    {
                        appliedChanges.Add(updateChange);
                    }
                }

                //updating product status
                try
                {
                    ProductModule.ChangeProductStatus(oldProduct.sku, ProductStatus.WaitingShopSync);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //reporting progress
                allProductsActionDone += 1;
                int promiles = (allProductsActionDone * 1000) / allProductsActionCount;
                worker.ReportProgress(promiles, $"Updating {TablePrefix} Products ({index + 1}/{updateProductsLenght}) - Total Actions ({allProductsActionDone}/{allProductsActionCount})");
            }

            //pass applied changes and pending changes to update on complete method
            Dictionary<string, object> changes = new();
            changes.Add("UpdatedProducts", appliedChanges);
            changes.Add("ArchivedProducts", archivedChanges);
            changes.Add("NewProducts", newChanges);
            if (e != null)
            {
                e.Result = changes;
            }
        }
    }
}
