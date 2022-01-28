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

            Dictionary<string, Dictionary<string, string>> appliedChanges = new();          //for updates
            List<Dictionary<string, string>> newChanges = new();                            //for new products
            List<Dictionary<string, string>> archivedChanges = new();                       //for archived Products                     

            //archiving products
            foreach (Product archiveProduct in ArchiveProducts)
            {
                try
                {
                    ProductModule.ChangeProductStatus(archiveProduct.sku, ProductStatus.NeedsArchiving);

                    Dictionary<string, string> archiveChange = new();
                    archiveChange.Add("SKU", archiveProduct.sku);
                    archiveChange.Add("PriceVendor", archiveProduct.productVariants.First().vendor_price.ToString());
                    archiveChange.Add("Price", archiveProduct.productVariants.First().price.ToString());
                    archiveChange.Add("Stock", archiveProduct.productVariants.First().stock.ToString());
                    archiveChange.Add("Barcode", archiveProduct.productVariants.First().barcode);
                    archiveChange.Add("Vendor", archiveProduct.vendor);
                    archiveChange.Add("VendorType", archiveProduct.productTypeVendor);
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

                Dictionary<string, string> newChange = new();
                newChange.Add("SKU", newProduct.sku);
                newChange.Add("PriceVendor", newProduct.productVariants.First().vendor_price.ToString());
                newChange.Add("Price", newProduct.productVariants.First().price.ToString());
                newChange.Add("Stock", newProduct.productVariants.First().stock.ToString());
                newChange.Add("Barcode", newProduct.productVariants.First().barcode);
                newChange.Add("Vendor", newProduct.vendor);
                newChange.Add("VendorType", newProduct.productTypeVendor);
                newChanges.Add(newChange);
            }

            DataBaseInterface db = new();

            //updating products
            foreach (Product updateProduct in UpdateProducts)
            {
                Product oldProduct = DBProducts.Find(x => x.sku == updateProduct.sku);

                foreach (ProductVariant pVariant in oldProduct.productVariants) {
                    ProductVariant dbVariant = oldProduct.productVariants.Where(x => x.barcode == pVariant.barcode).fi
                
                }

                //if no changes skip
                if (updateProduct.productVariants.First().stock == oldProduct.productVariants.First().stock && updateProduct.productVariants.First().vendor_price == oldProduct.productVariants.First().vendor_price)
                {
                    continue;
                }
                else
                {

                    appliedChanges.Add(oldProduct.sku, new Dictionary<string, string>()
                    {
                        ["Stock"] = "",
                        ["PriceVendor"] = "",
                        ["Price"] = "",
                    });

                    //update stock
                    if (updateProduct.productVariants.First().stock != oldProduct.productVariants.First().stock)
                    {
                        var stockUpdateData = new Dictionary<string, string>
                        {
                            ["Stock"] = updateProduct.productVariants.First().stock.ToString()
                        };
                        var stockWhereUpdate = new Dictionary<string, Dictionary<string, string>>
                        {
                            ["SKU"] = new Dictionary<string, string>
                            {
                                ["="] = oldProduct.sku
                            }
                        };
                        db.Table("_TDB_Variants").Where(stockWhereUpdate).Update(stockUpdateData);

                        //adding change to applied change list
                        appliedChanges[oldProduct.sku]["Stock"] = $"{oldProduct.productVariants.First().stock} -> {updateProduct.productVariants.First().stock}";
                    }

                    //update price
                    if (updateProduct.productVariants.First().vendor_price != oldProduct.productVariants.First().vendor_price)
                    {
                        //updating price value
                        var priceUpdateData = new Dictionary<string, string>
                        {
                            ["PriceVendor"] = updateProduct.productVariants.First().vendor_price.ToString(),
                            ["Price"] = updateProduct.productVariants.First().price.ToString()
                        };
                        var priceWhereUpdate = new Dictionary<string, Dictionary<string, string>>
                        {
                            ["SKU"] = new Dictionary<string, string>
                            {
                                ["="] = oldProduct.sku
                            }
                        };
                        db.Table("_TDB_Products").Where(priceWhereUpdate).Update(priceUpdateData);

                        //adding change to applied change list
                        appliedChanges[oldProduct.sku]["PriceVendor"] = $"{oldProduct.productVariants.First().vendor_price} -> {updateProduct.productVariants.First().vendor_price}";
                        appliedChanges[oldProduct.sku]["Price"] = $"{oldProduct.productVariants.First().price} -> {updateProduct.productVariants.First().price}";
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
