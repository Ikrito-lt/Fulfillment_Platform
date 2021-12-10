using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ikrito_Fulfillment_Platform {
    public partial class MainPage : Page {

        public static MainPage Instance { get; private set; }
        static MainPage() {
            Instance = new MainPage();
        }

        private readonly List<Order> newOrders = new();
        private readonly OrderModule newOrderGetter = new();

        private MainPage() {
            InitializeComponent();
            RefreshNewOrderDG();
        }

        //
        // Data grid manipulation section
        //

        //changes text to say howm much orders there is
        private void UpdateNewOrderLabel(int count) { 
            newOrderCountL.Content = $"Current Orders ({count})";
        }

        //changes text to say howm much orders there is
        private void UpdateProcessingOrderLabel(int count) {
            processingOrderCountL.Content = $"Processing Orders ({count})";
        }

        //loads orders to order grid
        private void RefreshNewOrderDG() {
            newOrderDG.ItemsSource = null;
            newOrders.Clear();
            newOrders.AddRange(newOrderGetter.getOrders());

            UpdateNewOrderLabel(newOrders.Count);
            newOrderDG.ItemsSource = newOrders.ToList();
        }


        //
        // Page change section
        // 

        //opens page with all products
        private void openBrowserPage(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = ProductBrowsePage.Instance;
        }

        //opens sync page
        private void openSyncPage(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = ProductSyncPage.Instance;
        }

        //opens order Info page
        private void Row_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            DataGridRow row = sender as DataGridRow;
            Order order = row.Item as Order;
            MainWindow.Instance.mainFrame.Content = new OrderInfoPage(order, this);
        }
    }
}
