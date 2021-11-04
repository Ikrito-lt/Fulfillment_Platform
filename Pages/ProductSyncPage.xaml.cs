using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
            LoadSyncProducts();
            Sync = new();
        }

        //method that creates BGW to load Sync products
        private void LoadSyncProducts() {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.DoWork += BGW_PreloadSyncProducts;
            worker.RunWorkerCompleted += BGW_PreloadSyncProductsCompleted;

            //blocking refresh button and animating loading bar
            progressBar.IsIndeterminate = true;
            RefreshButton.IsEnabled = false;

            worker.RunWorkerAsync();
        }
        //BGW load sync products
        private void BGW_PreloadSyncProducts(object sender, DoWorkEventArgs e) {
            List<SyncProduct> products = ProductSyncModule.GetSyncProducts();
            e.Result = products;
        }
        //BGW load sync products onComplete
        private void BGW_PreloadSyncProductsCompleted(object sender, RunWorkerCompletedEventArgs e) {
            SyncProducts = (List<SyncProduct>)e.Result;
            Sync.syncProducts = (List<SyncProduct>)e.Result;
            FilteredSyncProducts = SyncProducts;

            //init DataGrid
            productSyncDG.ItemsSource = FilteredSyncProducts;
            //init label
            ChangeCountLabel(FilteredSyncProducts.Count);
            //unblocking refresh button and unanimating loading bar
            progressBar.IsIndeterminate = false;
            RefreshButton.IsEnabled = true;
            Debug.WriteLine("BGW_PreloadAllProducts Finished");
        }
        
        private void ChangeCountLabel(int count) {
            SyncProductCountL.Content = "Sync Product Count: " + count.ToString();
        }
        //method removes SKU filter from the data grid
        private void deleteFilters() {
            SKUFilterSBox.Clear();
            queryLenght = 0;
            FilteredSyncProducts = SyncProducts;
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

        //button that goes back to main screen
        private void BackButton_Click(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = MainPage.Instance;
        }
        //button that refreshes the data grid
        private void RefreshButton_Click(object sender, RoutedEventArgs e) {
            deleteFilters();
            LoadSyncProducts();
        }
        //button that starts exporting products to Shopify
        private void UpdateProducts_Click(object sender, RoutedEventArgs e) {
            //running export products in background
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Sync.ExportShopifyProducts;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
        }

        //method that updates progress bar during product export
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            progressBar.Value = e.ProgressPercentage;
        }

        //button that updates product from TDB
        private void UpdateTDBButton_Click(object sender, RoutedEventArgs e) {
            TDBModule TDBUpdater = new();

            //running export products in background
            BackgroundWorker TDBUpdateWorker = new BackgroundWorker();
            TDBUpdateWorker.DoWork += TDBUpdater.updateTDBProducts;
            TDBUpdateWorker.RunWorkerCompleted += TDBUpdater.updateTDBProductsComplete;

            TDBUpdateWorker.RunWorkerAsync();
        }
    }
}
