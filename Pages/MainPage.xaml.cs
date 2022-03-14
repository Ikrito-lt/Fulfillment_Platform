using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Pages;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

        private MainPage()
        {
            InitializeComponent();
            LoadAllOrders();
        }

        private readonly List<Order> newOrders = new();
        private readonly List<Order> fulfilledOrders = new();

        //
        // Data grid manipulation section
        //

        /// <summary>
        /// changes text to say how much orders there is
        /// </summary>
        /// <param name="count"></param>
        private void UpdateNewOrderLabel(int count) { 
            newOrderCountL.Content = $"Current Orders ({count})";
        }

        /// <summary>
        /// changes text to say howm much fulfilled orders there is
        /// </summary>
        /// <param name="count"></param>
        private void UpdateFulfilledOrderLabel(int count) {
            fulfilledCountL.Content = $"Fulfiled Orders ({count})";
        }

        /// <summary>
        /// on click method of refresh button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshButton_Click(object sender, RoutedEventArgs e) {
            LoadAllOrders();
        }

        /// <summary>
        /// method for  loading products to datagrid
        /// </summary>
        public void LoadAllOrders() {
            BackgroundWorker OrderWorker = new();
            OrderWorker.WorkerReportsProgress = false;
            OrderWorker.DoWork += BGW_LoadAllOrders;
            OrderWorker.RunWorkerCompleted += BGW_LoadAllOrdersCompleted;

            //blocking refresh button and animating loading bar
            loadingBar.IsIndeterminate = true;
            RefreshButton.IsEnabled = false;
            loadingbarLabel.Text = "Loading Orders";

            OrderWorker.RunWorkerAsync();
        }

        /// <summary>
        /// backgroud worker for loading all Orders
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BGW_LoadAllOrders(object sender, DoWorkEventArgs e) {
            Dictionary<string, List<Order>> result = new();
            result.Add("newOrders", new List<Order>());
            result.Add("fulfilledOrders", OrderModule.getFulfilledOrders());

            e.Result = result;
        }

        /// <summary>
        /// background Worker for loading all orders on complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BGW_LoadAllOrdersCompleted(object sender, RunWorkerCompletedEventArgs e) {
            //refreshing datagrids
            var result = e.Result as Dictionary<string, List<Order>>;
            RefreshFulfilledOrderDG(result["fulfilledOrders"]);
            RefreshNewOrderDG(result["newOrders"]);

            //unblocking refresh button and unanimating loading bar
            loadingBar.IsIndeterminate = false;
            RefreshButton.IsEnabled = true;
            loadingbarLabel.Text = "";
            Debug.WriteLine("BGW_LoadAllOrders Finished");
        }

        /// <summary>
        /// loads orders to order grid
        /// </summary>
        /// <param name="orders"></param>
        private void RefreshFulfilledOrderDG(List<Order> orders) {
            fulfilledOrderDG.ItemsSource = null;
            fulfilledOrders.Clear();
            fulfilledOrders.AddRange(orders);

            UpdateFulfilledOrderLabel(fulfilledOrders.Count);
            fulfilledOrderDG.ItemsSource = fulfilledOrders.ToList();
        }

        /// <summary>
        /// loads orders to order grid
        /// </summary>
        /// <param name="orders"></param>
        private void RefreshNewOrderDG(List<Order> orders) {
            newOrderDG.ItemsSource = null;
            newOrders.Clear();
            newOrders.AddRange(orders);

            UpdateNewOrderLabel(newOrders.Count);
            newOrderDG.ItemsSource = newOrders.ToList();
        }


        //
        // Page change section
        // 

        /// <summary>
        /// opens page with all products
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openBrowserPage(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = ProductBrowsePage.Instance;
        }

        /// <summary>
        /// opens sync page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openSyncPage(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = ProductUpdatePage.Instance;
        }

        /// <summary>
        /// opens pigu integration page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openPiguIntegrationPage(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.mainFrame.Content = PiguIntegrationPage.Instance;
        }

        /// <summary>
        /// opens order Info page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Row_MouseDoubleClickCurrentOrder(object sender, MouseButtonEventArgs e) {
            DataGridRow row = sender as DataGridRow;
            Order order = row.Item as Order;
            MainWindow.Instance.mainFrame.Content = new OrderInfoPage(order, this);
        }

        /// <summary>
        /// opens order Info page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Row_MouseDoubleClickFulfilledOrder(object sender, MouseButtonEventArgs e) {
            DataGridRow row = sender as DataGridRow;
            Order order = row.Item as Order;
            MainWindow.Instance.mainFrame.Content = new OrderInfoPage(order, this, false);
        }

    }
}
