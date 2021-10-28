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
using Ikrito_Fulfillment_Platform.Utils;
using Ikrito_Fulfillment_Platform.Modules;
using Newtonsoft.Json;
using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Pages;
using System.ComponentModel;
using System.Diagnostics;

namespace Ikrito_Fulfillment_Platform {
    public partial class MainPage : Page {

        public static MainPage Instance { get; private set; }
        static MainPage() {
            Instance = new MainPage();
        }

        private List<Order> newOrders = new();
        private NewOrderModule newOrderGetter = new();
        private List<Product> AllProducts;

        private MainPage() {
            InitializeComponent();
            RefreshNewOrderDG();
            PreloadAllProducts();
        }

        private void UpdateNewOrderLabel(int count) { 
            newOrderCountL.Content = "Current New Orders: " + count.ToString();
        } 

        private void RefreshNewOrderDG() {

            newOrderDG.ItemsSource = null;
            newOrders.Clear();
            newOrders.AddRange(JsonConvert.DeserializeObject<List<Order>>(newOrderGetter.getOrders()));

            UpdateNewOrderLabel(newOrders.Count);

            newOrderDG.ItemsSource = newOrders;
        }

        private void ShowOrderInfo(object sender, RoutedEventArgs e) {
            ///button asdsadasdasd
        }

        private void PreloadAllProducts() {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.DoWork += BGW_PreloadAllProducts;
            worker.RunWorkerCompleted += BGW_PreloadAllProductsCompleted;
            worker.RunWorkerAsync();
        }

        private void BGW_PreloadAllProducts(object sender, DoWorkEventArgs e) {
            List<Product> products = ProductModule.GetAllProducts();
            e.Result = products;
        }

        private void BGW_PreloadAllProductsCompleted(object sender, RunWorkerCompletedEventArgs e) {
            AllProducts = (List<Product>)e.Result;
            Debug.WriteLine("BGW_PreloadAllProducts Finished");
        }

        private void openBrowserPage(object sender, RoutedEventArgs e) {
            if (AllProducts == null) {

            } else {

                MainWindow.Instance.mainFrame.Content = ProductBrowsePage.Instance;
            }
        }
    }
}
