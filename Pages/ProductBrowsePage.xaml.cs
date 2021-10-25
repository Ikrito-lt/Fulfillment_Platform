using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ikrito_Fulfillment_Platform.Pages {
    public partial class ProductBrowsePage : Page {

        private readonly List<Product> allProducts;

        private List<Product> filteredProducts;

        //shit that makes this a singelton
        public static ProductBrowsePage Instance { get; private set; }
        static ProductBrowsePage() {
            Instance = new ProductBrowsePage();
        }

        private ProductBrowsePage() {
            allProducts = GetProdList();
            InitializeComponent();

            filteredProducts = allProducts;

            //init DataGrid
            productDG.ItemsSource = filteredProducts;
            //init label
            ChangeCountLabel(filteredProducts.Count);
        }

        private static List<Product> GetProdList() {
            //getting products from TDB Module
            var TDBModule = new TDBModule();
            List<Product> TDBproducts = Modules.TDBModule.getProductsFromDB();

            // adding all products to one list
            List<Product> prodList = new();
            prodList.AddRange(TDBproducts);

            return prodList;
        }

        private void ChangeCountLabel(int count) {
            productCountL.Content = "Product Count: " + count.ToString();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = MainPage.Instance;
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = ProductSyncPage.Instance;
        }

        private void Row_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DataGridRow row = sender as DataGridRow;
            Product product = row.Item as Product;
            MainWindow.Instance.mainFrame.Content = new ProductEditPage(product);
        }

        //method for filtering by vendor
        private void VendorFilterSBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length >= 2) {

                string query = textBox.Text.ToLower();

                if (productDG.ItemsSource == filteredProducts) {
                    filteredProducts = filteredProducts.Where(p => p.vendor.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                } else {
                    filteredProducts = allProducts.Where(p => p.vendor.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(allProducts.Count);
                productDG.ItemsSource = allProducts;
            }
        }

        private void SKUFilterSBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length >= 2) {

                string query = textBox.Text.ToLower();

                if (productDG.ItemsSource == filteredProducts) {
                    filteredProducts = filteredProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                } else {
                    filteredProducts = allProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(allProducts.Count);
                productDG.ItemsSource = allProducts;
            }
        }

        private void TitleFilterSBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length >= 2) {

                string query = textBox.Text.ToLower();

                if (productDG.ItemsSource == filteredProducts) {
                    filteredProducts = filteredProducts.Where(p => p.title.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                } else {
                    filteredProducts = allProducts.Where(p => p.title.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(allProducts.Count);
                productDG.ItemsSource = allProducts;
            }
        }

        private void TypeFilterSBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length >= 2) {

                string query = textBox.Text.ToLower();

                if (productDG.ItemsSource == filteredProducts) {
                    filteredProducts = filteredProducts.Where(p => p.product_type.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                } else {
                    filteredProducts = allProducts.Where(p => p.product_type.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(filteredProducts.Count);
                    productDG.ItemsSource = filteredProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(allProducts.Count);
                productDG.ItemsSource = allProducts;
            }
        }
    }
}
