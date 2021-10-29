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

        private MainPage() {
            InitializeComponent();
            RefreshNewOrderDG();
        }

        private void UpdateNewOrderLabel(int count) { 
            newOrderCountL.Content = "Current New Orders: " + count.ToString();
        } 

        private void RefreshNewOrderDG() {

            newOrderDG.ItemsSource = null;
            newOrders.Clear();
            newOrders.AddRange(newOrderGetter.getOrders());

            UpdateNewOrderLabel(newOrders.Count);

            newOrderDG.ItemsSource = newOrders;
        }

        private void ShowOrderInfo(object sender, RoutedEventArgs e) {
            ///button asdsadasdasd
        }

        private void openBrowserPage(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = ProductBrowsePage.Instance;
        }
    }
}
