using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Utils;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ikrito_Fulfillment_Platform.Pages {
    public partial class ProductSyncPage : Page {

        private readonly ProductSyncModule Sync;
        private List<SyncProduct> SyncProducts;
        private List<SyncProduct> FilteredSyncProducts;

        private int queryLenght = 0;
        private bool _clearFilters;
        public bool clearFilters {
            get { return _clearFilters; }
            set {
                _clearFilters = value;
                if (value == true) {
                    deleteFilters();
                }
            }
        }

        //shit makes it a singleton
        public static ProductSyncPage Instance { get; private set; }
        static ProductSyncPage() {
            Instance = new ProductSyncPage();
        }

        private ProductSyncPage() {
            InitializeComponent();

            //getting SyncProducts
            Sync = new();
            SyncProducts = Sync.syncProducts;
            FilteredSyncProducts = SyncProducts;
           
            //init DataGrid
            productSyncDG.ItemsSource = FilteredSyncProducts;
            
            //init label
            ChangeCountLabel(FilteredSyncProducts.Count);
        }

        private void deleteFilters() {
            SKUFilterSBox.Clear();
            queryLenght = 0;
            FilteredSyncProducts = SyncProducts;
        }

        private void ChangeCountLabel(int count) {
            SyncProductCountL.Content = "Sync Product Count: " + count.ToString();
        }

        //method for fitering by sku
        private void SKUFilterSBox_TextChanged(object sender, TextChangedEventArgs e) {
            TextBox textBox = sender as TextBox;
            
            int currentQueryLenght = textBox.Text.Length;
            if (currentQueryLenght < queryLenght) {
                clearFilters = true;
            } else {
                queryLenght = currentQueryLenght;
            }

            if (textBox.Text.Length >= 2) {
                string query = textBox.Text.ToLower();

                if (productSyncDG.ItemsSource == FilteredSyncProducts) {
                    FilteredSyncProducts = FilteredSyncProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(FilteredSyncProducts.Count);
                    productSyncDG.ItemsSource = FilteredSyncProducts;
                } else {
                    FilteredSyncProducts = SyncProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(FilteredSyncProducts.Count);
                    productSyncDG.ItemsSource = FilteredSyncProducts;
                }
            } else if (textBox.Text.Length == 0) {
                ChangeCountLabel(SyncProducts.Count);
                productSyncDG.ItemsSource = SyncProducts;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = ProductBrowsePage.Instance;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) {
            deleteFilters();
            //getting SyncProducts
            SyncProducts = Sync.RefreshSyncProducts();
            FilteredSyncProducts = SyncProducts;
            productSyncDG.ItemsSource = FilteredSyncProducts;
            ChangeCountLabel(FilteredSyncProducts.Count);
        }

        private void UpdateProducts_Click(object sender, RoutedEventArgs e) {
            //running export products in background
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Sync.ExportShopifyProducts;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            loadingBar.Value = e.ProgressPercentage;
        }
    }
}
