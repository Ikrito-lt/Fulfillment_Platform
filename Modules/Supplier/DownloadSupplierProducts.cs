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
using static Ikrito_Fulfillment_Platform.Models.FullProduct;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier
{
    static class DownloadSupplierProducts
    {

        /// <summary>
        /// gets supplier products using their TablePrefix
        /// </summary>
        /// <param name="TablePrefix"></param>
        /// <returns></returns>
        private static List<FullProduct> GetSupplierProductList(string TablePrefix) {
            List<FullProduct> pList = new();

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
            //for reporting
            int promiles = 0;
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.ReportProgress(promiles, (true, $"Updating {TablePrefix} products"));

            //getting DB Products
            Dictionary<string, FullProduct> DBProducts = new Dictionary<string, FullProduct>();
            foreach ((string sku, FullProduct DBProduct) in ProductModule.GetVendorProducts(TablePrefix)) {
                DBProducts.Add(sku, DBProduct);
            }

            //getting API products
            Dictionary<string, FullProduct> APIProducts = new Dictionary<string, FullProduct>();
            foreach (FullProduct APIProduct in GetSupplierProductList(TablePrefix)) {
                APIProducts.Add(APIProduct.SKU, APIProduct);
            }

            //sorting
            List<string> ArchiveProductSKUs = DBProducts.Keys.Except(APIProducts.Keys).ToList();
            List<string> UpdateProductSKUs = DBProducts.Keys.Intersect(APIProducts.Keys).ToList();
            List<string> NewProductSKUs = APIProducts.Keys.Except(DBProducts.Keys).ToList();

            List<FullProduct> ArchiveProducts = DBProducts.Values.Where(x => ArchiveProductSKUs.Contains(x.SKU)).ToList();
            List<FullProduct> NewProducts = APIProducts.Values.Where(x => NewProductSKUs.Contains(x.SKU)).ToList();
            
            //todo: i need to see what products in updateProducts list i actually need to update to make this number exact
            List<FullProduct> UpdateProducts = APIProducts.Values.Where(x => UpdateProductSKUs.Contains(x.SKU)).ToList();
            
            //for reporting progress in listboxes
            List<ProductChangeRecord> appliedChanges = new();          //for updates
            List<ProductChangeRecord> newChanges = new();              //for new products
            List<ProductChangeRecord> archivedChanges = new();         //for archived Products                     

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
                    ProductModule.ChangeProductStatus(archiveProduct.SKU, ProductStatus.Archived, false);
                    ProductVariant firstVariant = archiveProduct.ProductVariants.First();

                    ProductChangeRecord archiveChange = new ProductChangeRecord {
                        SKU = archiveProduct.SKU,
                        PriceVendor = firstVariant.PriceVendor.ToString(),
                        Price = firstVariant.Price.ToString(),
                        Stock = firstVariant.Price.ToString(),
                        Barcode = firstVariant.Barcode.ToString(),
                        Vendor = archiveProduct.Vendor,
                        VendorProductType = archiveProduct.ProductTypeVendor,
                        ProductType = archiveProduct.ProductTypeDisplayVal,
                        Status = archiveProduct.Status,
                        VariantData = $"{firstVariant.VariantType} - {firstVariant.VariantData}",
                        VariantCount = archiveProduct.ProductVariants.Count(),
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
                promiles = (allProductsActionDone * 1000) / allProductsActionCount;
                int percents = promiles / 10;
                worker.ReportProgress(promiles, (false, $"Archiving {TablePrefix} Products ({index + 1}/{archiveProductsLenght}) - Total Actions ({allProductsActionDone}/{allProductsActionCount}) {percents}%"));
            }

            // adding new Products
            foreach (var (newProduct, index) in NewProducts.LoopIndex())
            {
                ProductModule.AddProductToDB(newProduct);

                ProductVariant firstVariant = newProduct.ProductVariants.First();
                ProductChangeRecord newChange = new ProductChangeRecord
                {
                    SKU = newProduct.SKU,
                    PriceVendor = firstVariant.PriceVendor.ToString(),
                    Price = firstVariant.Price.ToString(),
                    Stock = firstVariant.Price.ToString(),
                    Barcode = firstVariant.Barcode.ToString(),
                    Vendor = newProduct.Vendor,
                    VendorProductType = newProduct.ProductTypeVendor,
                    ProductType = newProduct.ProductTypeDisplayVal,
                    Status = newProduct.Status,
                    VariantData = $"{firstVariant.VariantType} - {firstVariant.VariantData}",
                    VariantCount = newProduct.ProductVariants.Count(),
                    ChangesMade = new()
                };
                newChange.ChangesMade.Add("Added to Database");
                newChanges.Add(newChange);

                //reporting progress
                allProductsActionDone += 1;
                promiles = (allProductsActionDone * 1000) / allProductsActionCount;
                int percents = promiles / 10;
                worker.ReportProgress(promiles, (false, $"Adding New {TablePrefix} Products ({index + 1}/{newProductsLenght}) - Total Actions ({allProductsActionDone}/{allProductsActionCount}) {percents}%"));
            }

            //updating products
            DataBaseInterface db = new();
            foreach (var (updateProduct, index) in UpdateProducts.LoopIndex())
            {
                //updating only product variants and prices not title description etc
                //todo: maybe auto update images
                FullProduct oldProduct = DBProducts[updateProduct.SKU];

                foreach (ProductVariant pVariant in updateProduct.ProductVariants) {
                    ProductVariant dbVariant = oldProduct.ProductVariants.Where(x => x.Barcode == pVariant.Barcode).FirstOrDefault();

                    ProductChangeRecord updateChange = new ProductChangeRecord
                    {
                        SKU = updateProduct.SKU,
                        Barcode = pVariant.Barcode.ToString(),
                        Vendor = updateProduct.Vendor,
                        VendorProductType = updateProduct.ProductTypeVendor,
                        ProductType = updateProduct.ProductTypeDisplayVal,
                        Status = updateProduct.Status,
                        VariantCount = updateProduct.ProductVariants.Count(),
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
                                    ["Price"] = dbVariant.Price.ToString(),
                                    ["PriceVendor"] = pVariant.PriceVendor.ToString(),
                                    ["Stock"] = pVariant.Stock.ToString(),
                                    ["VariantType"] = pVariant.VariantType,
                                    ["VariantData"] = pVariant.VariantData,
                                };
                                var variantWhereUpdate = new Dictionary<string, Dictionary<string, string>>
                                {
                                    ["ID"] = new Dictionary<string, string>
                                    {
                                        ["="] = dbVariant.VariantDBID.ToString()
                                    }
                                };
                                db.Table($"_{TablePrefix}_Variants").Where(variantWhereUpdate).Update(variantUpdateData);

                                updateChange.Price = dbVariant.Price.ToString();
                                updateChange.PriceVendor = pVariant.PriceVendor.ToString();
                                updateChange.Stock = pVariant.Stock.ToString();
                                updateChange.VariantData = $"{pVariant.VariantType} - {pVariant.VariantData}";
                                updateChange.ChangesMade.Add($"Changed stock:\t\t{dbVariant.Stock} -> {pVariant.Stock}");
                                updateChange.ChangesMade.Add($"Price is permament check profit!");
                                updateChange.ChangesMade.Add($"Changed vendor price:\t\t{dbVariant.PriceVendor} -> {pVariant.PriceVendor}");
                            }
                            else {
                                //price isnt permament
                                var variantUpdateData = new Dictionary<string, string>
                                {
                                    ["Price"] = pVariant.Price.ToString(),
                                    ["PriceVendor"] = pVariant.PriceVendor.ToString(),
                                    ["Stock"] = pVariant.Stock.ToString(),
                                    ["VariantType"] = pVariant.VariantType,
                                    ["VariantData"] = pVariant.VariantData,
                                };
                                var variantWhereUpdate = new Dictionary<string, Dictionary<string, string>>
                                {
                                    ["ID"] = new Dictionary<string, string>
                                    {
                                        ["="] = dbVariant.VariantDBID.ToString()
                                    }
                                };
                                db.Table($"_{TablePrefix}_Variants").Where(variantWhereUpdate).Update(variantUpdateData);

                                updateChange.Price = dbVariant.Price.ToString();
                                updateChange.PriceVendor = pVariant.PriceVendor.ToString();
                                updateChange.Stock = pVariant.Stock.ToString();
                                updateChange.VariantData = $"{pVariant.VariantType} - {pVariant.VariantData}";
                                updateChange.ChangesMade.Add($"Changed stock:\t\t{dbVariant.Stock} -> {pVariant.Stock}");
                                updateChange.ChangesMade.Add($"Changed price:\t\t{dbVariant.Price} -> {pVariant.Price}");
                                updateChange.ChangesMade.Add($"Changed vendor price:\t\t{dbVariant.PriceVendor} -> {pVariant.PriceVendor}");
                            }
                        }
                    }
                    else {
                        //check if there are any variants with out barcode in this product and delete them
                        var barcodeLessVariants = oldProduct.ProductVariants.Where(x => string.IsNullOrEmpty(x.Barcode));
                        foreach (ProductVariant barcodeLessVariant in barcodeLessVariants) {
                            var variantWhereDelete = new Dictionary<string, Dictionary<string, string>>
                            {
                                ["ID"] = new Dictionary<string, string>
                                {
                                    ["="] = barcodeLessVariant.VariantDBID.ToString()
                                }
                            };
                            db.Table($"_{TablePrefix}_Variants").Where(variantWhereDelete).Delete();
                        }

                        // add new variant
                        var insertData = new Dictionary<string, string>
                        {
                            ["SKU"] = updateProduct.SKU,
                            ["Price"] = pVariant.Price.ToString(),
                            ["PriceVendor"] = pVariant.PriceVendor.ToString(),
                            ["Stock"] = pVariant.Stock.ToString(),
                            ["Barcode"] = pVariant.Barcode,
                            ["VariantType"] = pVariant.VariantType,
                            ["VariantData"] = pVariant.VariantData,
                            ["PermPrice"] = pVariant.PermPrice ? "1" : "0"
                        };
                        db.Table($"_{TablePrefix}_Variants").Insert(insertData);

                        updateChange.Price = pVariant.Price.ToString();
                        updateChange.PriceVendor = pVariant.PriceVendor.ToString();
                        updateChange.Stock = pVariant.Stock.ToString();
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
                    //cause new status only changes by button
                    if (oldProduct.Status != ProductStatus.New)
                    {
                        //checking if product isn out of stock
                        if (ProductModule.CheckIfProductOutOfStock(updateProduct)) {
                            ProductModule.ChangeProductStatus(oldProduct.SKU, ProductStatus.Ok, false);
                        }
                        else {
                            ProductModule.ChangeProductStatus(oldProduct.SKU, ProductStatus.OutOfStock, false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                //reporting progress
                allProductsActionDone += 1;
                promiles = (allProductsActionDone * 1000) / allProductsActionCount;
                int percents = promiles / 10;
                worker.ReportProgress(promiles, (false, $"Updating {TablePrefix} Products ({index + 1}/{updateProductsLenght}) - Total Actions ({allProductsActionDone}/{allProductsActionCount}) {percents}%"));
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
