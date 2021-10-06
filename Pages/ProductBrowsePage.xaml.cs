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

        private readonly List<Product> products;

        //shit that makes this a singelton
        public static ProductBrowsePage Instance { get; private set; }
        static ProductBrowsePage() {
            Instance = new ProductBrowsePage();
        }

        private ProductBrowsePage() {
            products = loadProdList();
            InitializeComponent();

            //init DataGrid
            productDG.ItemsSource = products;
        }

        private List<Product> loadProdList() {
            //getting products from TDB Module
            var TDBModule = new TDBModule();
            List<Product> TDBproducts = TDBModule.getProductsFromDB();

            // adding all products to one list
            List<Product> prodList = new();
            prodList.AddRange(TDBproducts);

            return prodList;
        }

        private void backButton_Click(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = MainPage.Instance;
        }

        private void Row_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DataGridRow row = sender as DataGridRow;
            Product product = row.Item as Product;
            MainWindow.Instance.mainFrame.Content = new ProductEditPage(product);
        }
    }
}
