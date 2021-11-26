using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Models.SyncModels;
using Ikrito_Fulfillment_Platform.Modules;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

//todo: when double ckicking any of these list boxes you edit product and then delete if from list box

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

            //NewProductListBox.MouseDoubleClick += ChangeListBox_MouseDoubleClick;
            //UpdatedProductListBox.MouseDoubleClick += ChangeListBox_MouseDoubleClick;
        }


        //
        // General method section
        //

        //method that changes datagrid count label text value
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


        //
        // section for loading Sync Products to datagrid
        //

        //method that creates BGW to load Sync products
        private void LoadSyncProducts() {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.DoWork += BGW_PreloadSyncProducts;
            worker.RunWorkerCompleted += BGW_PreloadSyncProductsCompleted;

            //blocking refresh button and animating loading bar
            progressBar.IsIndeterminate = true;
            RefreshButton.IsEnabled = false;

            progressBar.IsIndeterminate = true;
            progressBarLabel.Text = "Loading Sync Products from DataBase";

            worker.RunWorkerAsync();
        }

        //BGW load sync products
        private void BGW_PreloadSyncProducts(object sender, DoWorkEventArgs e) {
            List<SyncProduct> products = ProductSyncModule.GetSyncProducts();
            e.Result = products;
        }

        //BGW load sync products onComplete
        private void BGW_PreloadSyncProductsCompleted(object sender, RunWorkerCompletedEventArgs e) {
            //changing loading bar state
            progressBar.IsIndeterminate = false;
            progressBarLabel.Text = "";


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


        //
        // shopify sync section 
        //

        //button that starts shopify sync
        private void SyncProducts_Click(object sender, RoutedEventArgs e) {
            //running export products in background
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += Sync.ExportShopifyProducts;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
        }

        //method that updates progress bar during product export
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            int progress = e.ProgressPercentage;
            progressBar.Value = progress;
            progressBarLabel.Text = $"Syncing Products To Shopify: {progress}‰";

        }


        //
        // update TDB Products section
        //

        //button that updates product from TDB
        private void UpdateTDBButton_Click(object sender, RoutedEventArgs e) {
            TDBModule TDBUpdater = new();

            //running export products in background
            BackgroundWorker TDBUpdateWorker = new BackgroundWorker();
            TDBUpdateWorker.DoWork += TDBUpdater.updateTDBProducts;
            TDBUpdateWorker.RunWorkerCompleted += UpdateTDBWorkerOnComplete;

            progressBar.IsIndeterminate = true;
            progressBarLabel.Text = "Updating TDB products";
            TDBUpdateWorker.RunWorkerAsync();
        }

        //method that opens new dialogue window that shows all changes made in database
        private void UpdateTDBWorkerOnComplete(object sender, RunWorkerCompletedEventArgs e) {
            progressBar.IsIndeterminate = false;
            progressBarLabel.Text = "";

            PopulateChangeListBoxes(e.Result);
        }


        //
        // Changes ListBoxes section
        //

        //method that populates chnaged products listboxes
        private void PopulateChangeListBoxes(object Changes) {
            Dictionary<string, object> ChangesKVP = Changes as Dictionary<string, object>;

            //converting chnages to list to lists of productchanges
            List<Dictionary<string, string>> newProducts = ChangesKVP["NewProducts"] as List<Dictionary<string, string>>;                                           //what new product were added
            List<Dictionary<string, string>> invalidProducts = ChangesKVP["InvalidProducts"] as List<Dictionary<string, string>>;                                   //what products werent added because they were missing datasheet
            Dictionary<string, Dictionary<string, string>> updatedProducts = ChangesKVP["UpdatedProducts"] as Dictionary<string, Dictionary<string, string>>;       //what products were changed

            List<ProductChange> NewProducts = new();
            List<ProductChange> InvalidProducts = new();
            List<ProductChange> UpdatedProducts = new();

            foreach (var NP in newProducts) {
                ProductChange NewProduct = new();
                NewProduct.SKU = NP["SKU"];
                NewProduct.PriceVendor = NP["PriceVendor"];
                NewProduct.Stock = NP["Stock"];
                NewProduct.Barcode = NP["Barcode"];
                NewProduct.Vendor = NP["Vendor"];
                NewProduct.VendorType = NP["VendorType"];

                NewProducts.Add(NewProduct);
            }
            foreach (var IP in invalidProducts) {
                ProductChange InvalidProduct = new();
                InvalidProduct.SKU = IP["SKU"];
                InvalidProduct.PriceVendor = IP["PriceVendor"];
                InvalidProduct.Stock = IP["Stock"];
                InvalidProduct.Barcode = IP["Barcode"];
                InvalidProduct.Vendor = IP["Vendor"];
                InvalidProduct.VendorType = IP["VendorType"];

                InvalidProducts.Add(InvalidProduct);
            }
            foreach (var UP in updatedProducts) {
                ProductChange ChangedProduct = new();
                ChangedProduct.SKU = UP.Key;
                ChangedProduct.Changes = UP.Value;

                UpdatedProducts.Add(ChangedProduct);
            }

            NewProductListBox.ItemsSource = NewProducts;
            UpdatedProductListBox.ItemsSource = UpdatedProducts;
            InvalidProductListBox.ItemsSource = InvalidProducts;
        }

        //method that allows user to edit list box product by opening it in ProductEditPage
        private void ChangeListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            ListBoxItem listboxItem = sender as ListBoxItem;
            
            if (listboxItem != null) {
                ProductChange productChange = listboxItem.Content as ProductChange;
                Product editProduct = ProductModule.GetProduct(productChange.SKU);
                MainWindow.Instance.mainFrame.Content = new ProductEditPage(editProduct, this);
            }
        }
    }
}
