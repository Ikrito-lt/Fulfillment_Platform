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
using System.Text;
using System.Threading.Tasks;
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
                //pList.AddRange(BFModule.)
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
            List<Product> DBProducts = ProductModule.GetVendorProducts(TablePrefix);
            List<Product> APIProducts =  GetSupplierProductList(TablePrefix);

            List<Product> ArchiveProducts = DBProducts.Where(p1 => APIProducts.All(p2 => p2.sku != p1.sku)).ToList();
            List<Product> NewProducts = APIProducts.Where(p1 => DBProducts.All(p2 => p2.sku != p1.sku)).ToList();
            List<Product> UpdateProducts = APIProducts.Where(p1 => NewProducts.All(p2 => p2.sku != p1.sku)).ToList();

            //remove dublicate skus from newProd list
            var a = NewProducts.GroupBy(x => x.sku.ToLower()).Where(x => x.LongCount() > 1).ToList();
            a.ForEach(x => NewProducts.RemoveAll(y => y.sku.ToLower() == x.Key));

            List<ProductChangeRecord> appliedChanges = new();          //for updates
            List<ProductChangeRecord> newChanges = new();                            //for new products
            List<ProductChangeRecord> archivedChanges = new();                       //for archived Products                     

            //archiving products
            foreach (Product archiveProduct in ArchiveProducts)
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
                        VariantCount = archiveProduct.productVariants.Count()
                    };
                    archiveChange.ChangesMade.Add("Archived");
                    archivedChanges.Add(archiveChange);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.",
                        "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // adding new Products
            foreach (Product newProduct in NewProducts)
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
                    VariantCount = newProduct.productVariants.Count()
                };
                newChange.ChangesMade.Add("Added to Database");
                newChanges.Add(newChange);
            }

            //updating products
            DataBaseInterface db = new();
            foreach (Product updateProduct in UpdateProducts)
            {
                Product oldProduct = DBProducts.Find(x => x.sku == updateProduct.sku);

                foreach (ProductVariant pVariant in oldProduct.productVariants) {
                    ProductVariant dbVariant = oldProduct.productVariants.Where(x => x.barcode == pVariant.barcode).FirstOrDefault();

                    ProductChangeRecord updateChange = new ProductChangeRecord
                    {
                        SKU = updateProduct.sku,
                        Barcode = pVariant.barcode.ToString(),
                        Vendor = updateProduct.vendor,
                        VendorProductType = updateProduct.productTypeVendor,
                        ProductType = updateProduct.ProductTypeDisplayVal,
                        Status = updateProduct.status,
                        VariantCount = updateProduct.productVariants.Count()
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
                                updateChange.ChangesMade.Add($"Changed stock: {dbVariant.stock} -> {pVariant.stock}");
                                updateChange.ChangesMade.Add($"Changed vendor price: {dbVariant.vendor_price} -> {pVariant.vendor_price}");
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
                                updateChange.ChangesMade.Add($"Changed stock: {dbVariant.stock} -> {pVariant.stock}");
                                updateChange.ChangesMade.Add($"Changed price: {dbVariant.price} -> {pVariant.price}");
                                updateChange.ChangesMade.Add($"Changed vendor price: {dbVariant.vendor_price} -> {pVariant.vendor_price}");
                            }
                        }
                    }
                    else {
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

                        updateChange.Price = dbVariant.price.ToString();
                        updateChange.PriceVendor = pVariant.vendor_price.ToString();
                        updateChange.Stock = pVariant.stock.ToString();
                        updateChange.VariantData = $"{pVariant.VariantType} - {pVariant.VariantData}";
                        updateChange.ChangesMade.Add($"Added Variant");
                    }
                
                    appliedChanges.Add(updateChange);
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
