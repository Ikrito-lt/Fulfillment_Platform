using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Pages;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

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
            //Todo: redo button
        }

        private void openBrowserPage(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = ProductBrowsePage.Instance;
        }

        //opens sync page
        private void openSyncPage(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = ProductSyncPage.Instance;
        }
    }
}
