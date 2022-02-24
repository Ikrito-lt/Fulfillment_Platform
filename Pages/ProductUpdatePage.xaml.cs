using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Modules.Supplier;
using Ikrito_Fulfillment_Platform.Modules.Supplier.KotrynaGroup;
using Ikrito_Fulfillment_Platform.Modules.Supplier.Pretendentas;
using Ikrito_Fulfillment_Platform.Modules.Supplier.TDBaltic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

//todo: when double ckicking any of these list boxes you edit product and then delete if from list box

namespace Ikrito_Fulfillment_Platform.Pages
{
    public partial class ProductUpdatePage : Page
    {
        private List<ProductState> UpdateProducts;
        private List<ProductState> FilteredUpdateProducts;

        private int queryLenght = 0;
        private bool _clearFilters;
        public bool clearFilters
        {
            get { return _clearFilters; }
            set
            {
                _clearFilters = value;
                if (value == true)
                {
                    deleteFilters();
                }
            }
        }

        //shit makes it a singleton
        public static ProductUpdatePage Instance { get; private set; }
        static ProductUpdatePage()
        {
            Instance = new ProductUpdatePage();
        }

        private ProductUpdatePage()
        {
            InitializeComponent();
            //getting SyncProducts
            LoadUpdateProducts();
        }


        //
        // General method section
        //

        //method that changes datagrid count label text value
        private void ChangeCountLabel(int count)
        {
            SyncProductCountL.Content = "Sync Product Count: " + count.ToString();
        }

        //method removes SKU filter from the data grid
        private void deleteFilters()
        {
            SKUFilterSBox.Clear();
            queryLenght = 0;
            FilteredUpdateProducts = UpdateProducts;
        }

        //method for fitering by sku
        private void SKUFilterSBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            int currentQueryLenght = textBox.Text.Length;
            if (currentQueryLenght < queryLenght)
            {
                clearFilters = true;
            }
            else
            {
                queryLenght = currentQueryLenght;
            }

            if (textBox.Text.Length >= 2)
            {
                string query = textBox.Text.ToLower();

                if (productSyncDG.ItemsSource == FilteredUpdateProducts)
                {
                    FilteredUpdateProducts = FilteredUpdateProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(FilteredUpdateProducts.Count);
                    productSyncDG.ItemsSource = FilteredUpdateProducts;
                }
                else
                {
                    FilteredUpdateProducts = UpdateProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                    ChangeCountLabel(FilteredUpdateProducts.Count);
                    productSyncDG.ItemsSource = FilteredUpdateProducts;
                }
            }
            else if (textBox.Text.Length == 0)
            {
                ChangeCountLabel(UpdateProducts.Count);
                productSyncDG.ItemsSource = UpdateProducts;
            }
        }

        //button that goes back to main screen
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.mainFrame.Content = MainPage.Instance;
        }

        //button that refreshes the data grid
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            deleteFilters();
            LoadUpdateProducts();
        }


        //
        // section for loading Sync Products to datagrid
        //

        //method that creates BGW to load Sync products
        private void LoadUpdateProducts()
        {
            BackgroundWorker worker = new();
            worker.WorkerReportsProgress = false;

            //blocking refresh button and animating loading bar pre lauching BGW
            progressBar.IsIndeterminate = true;
            RefreshButton.IsEnabled = false;
            progressBarLabel.Text = "Loading Sync Products from DataBase";

            //BGW DoWork
            worker.DoWork += (sender, e) => {
                List<ProductState> products = ProductUpdateModule.GetSyncProducts();
                e.Result = products;
            };

            // after BGW DoWork
            worker.RunWorkerCompleted += (sender, e) => {
                //changing loading bar state
                progressBar.IsIndeterminate = false;
                progressBarLabel.Text = "";

                UpdateProducts = (List<ProductState>)e.Result;
                FilteredUpdateProducts = UpdateProducts;

                //init DataGrid
                productSyncDG.ItemsSource = FilteredUpdateProducts;
                //init label
                ChangeCountLabel(FilteredUpdateProducts.Count);
                //unblocking refresh button and unanimating loading bar
                progressBar.IsIndeterminate = false;
                RefreshButton.IsEnabled = true;
                Debug.WriteLine("BGW_PreloadAllProducts Finished");
            };

            worker.RunWorkerAsync();
        }
        //
        // set stock 0 to archival
        //

        public void ChangeStatusByStock_Click(object sender = null, RoutedEventArgs e = null)
        {
            //BGW setup 
            BackgroundWorker worker = new();
            RefreshButton.IsEnabled = false;
            progressBar.Maximum = 1000;
            progressBarLabel.Text = "Changing Product Status If Stock > 0";

            //BGW progress reporting
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += (sender, e) =>
            {
                (bool makeProgressBarIndeterminate, string barText) = (ValueTuple<bool, string>)e.UserState;
                progressBar.IsIndeterminate = makeProgressBarIndeterminate;
                progressBarLabel.Text = barText;

                int progress = e.ProgressPercentage;
                progressBar.Value = progress;

            };

            //BGW doWork
            worker.DoWork += (sender, e) => ProductUpdateModule.ChangeStatusByStock(sender, e);

            //BGW after DoWork
            worker.RunWorkerCompleted += (sender, e) => {
                //changing loading bar state
                progressBar.Value = 0;
                progressBar.Maximum = 100;
                progressBarLabel.Text = "";

                LoadUpdateProducts();
            };

            worker.RunWorkerAsync();
        }


        //
        // Changes ListBoxes section
        //

        //method taht clears changes window
        private void ClearChangesListBoxes()
        {
            NewProductListBox.ItemsSource = null;
            UpdatedProductListBox.ItemsSource = null;
            ArchivedProductListBox.ItemsSource = null;

            NewProductsLabel.Content = $"New Products";
            UpdatedProductsLabel.Content = $"Updated Products";
            ArchivedProductsLabel.Content = $"Archived Products";
        }

        //method that populates chnaged products listboxes
        private void PopulateChangeListBoxes(object Changes)
        {
            Dictionary<string, object> ChangesKVP = Changes as Dictionary<string, object>;

            //converting chnages to list to lists of productchanges
            List<ProductChangeRecord> newProducts = ChangesKVP["NewProducts"] as List<ProductChangeRecord>;                     //what new product were added
            List<ProductChangeRecord> archivedProducts = ChangesKVP["ArchivedProducts"] as List<ProductChangeRecord>;           //what products werent added because they were missing datasheet
            List<ProductChangeRecord> updatedProducts = ChangesKVP["UpdatedProducts"] as List<ProductChangeRecord>;             //what products were changed

            NewProductListBox.ItemsSource = newProducts;
            UpdatedProductListBox.ItemsSource = updatedProducts;
            ArchivedProductListBox.ItemsSource = archivedProducts;

            NewProductsLabel.Content = $"New Products ({newProducts.Count})";
            UpdatedProductsLabel.Content = $"Updated Products ({updatedProducts.Count})";
            ArchivedProductsLabel.Content = $"Archived Products ({archivedProducts.Count})";
        }

        //method that allows user to edit list box product by opening it in ProductEditPage
        private void ChangeListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem listboxItem)
            {
                ProductChangeRecord productChange = listboxItem.Content as ProductChangeRecord;
                FullProduct editProduct = ProductModule.GetProduct(productChange.SKU);
                MainWindow.Instance.mainFrame.Content = new ProductEditPage(editProduct, this, ProductCategoryModule.Instance.CategoryKVP);
            }
        }


        //
        // update Vendor Products section
        //

        //button that updates product from TDB
        private void UpdateTDBButton_Click(object sender, RoutedEventArgs e)
        {
            //running export products in background
            BackgroundWorker TDBUpdateWorker = new();
            TDBUpdateWorker.WorkerReportsProgress = true;
            TDBUpdateWorker.DoWork += (sender, e) => DownloadSupplierProducts.UpdateProducts("TDB" ,sender, e);
            TDBUpdateWorker.RunWorkerCompleted += UpdateVendorProductsWorkerOnComplete;
            TDBUpdateWorker.ProgressChanged += UpdateVendorProductsWorkerProgressChanged;

            RefreshButton.IsEnabled = false;

            ClearChangesListBoxes();

            TDBUpdateWorker.RunWorkerAsync();
        }

        //button that updates product from KG
        private void UpdateKGButton_Click(object sender, RoutedEventArgs e)
        {
            //running export products in background
            BackgroundWorker KGUpdateWorker = new();
            KGUpdateWorker.WorkerReportsProgress = true;
            KGUpdateWorker.DoWork += (sender, e) => DownloadSupplierProducts.UpdateProducts("KG", sender, e);
            KGUpdateWorker.RunWorkerCompleted += UpdateVendorProductsWorkerOnComplete;
            KGUpdateWorker.ProgressChanged += UpdateVendorProductsWorkerProgressChanged;

            RefreshButton.IsEnabled = false;

            ClearChangesListBoxes();

            KGUpdateWorker.RunWorkerAsync();
        }

        //button that updates product from PD
        private void UpdatePDButton_Click(object sender, RoutedEventArgs e)
        {
            //running export products in background
            BackgroundWorker PDUpdateWorker = new();
            PDUpdateWorker.WorkerReportsProgress = true;
            PDUpdateWorker.DoWork += (sender, e) => DownloadSupplierProducts.UpdateProducts("PD", sender, e);
            PDUpdateWorker.RunWorkerCompleted += UpdateVendorProductsWorkerOnComplete;
            PDUpdateWorker.ProgressChanged += UpdateVendorProductsWorkerProgressChanged;

            RefreshButton.IsEnabled = false;

            ClearChangesListBoxes();

            PDUpdateWorker.RunWorkerAsync();
        }

        //button that updates product from BF supplier
        private void UpdateBFButton_Click(object sender, RoutedEventArgs e)
        {
            //running export products in background
            BackgroundWorker PDUpdateWorker = new();
            PDUpdateWorker.WorkerReportsProgress = true;
            PDUpdateWorker.DoWork += (sender, e) => DownloadSupplierProducts.UpdateProducts("BF", sender, e);
            PDUpdateWorker.RunWorkerCompleted += UpdateVendorProductsWorkerOnComplete;
            PDUpdateWorker.ProgressChanged += UpdateVendorProductsWorkerProgressChanged;

            RefreshButton.IsEnabled = false;

            ClearChangesListBoxes();

            PDUpdateWorker.RunWorkerAsync();
        }

        //method that updates progress bar during product export
        private void UpdateVendorProductsWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            (bool makeProgressBarIndeterminate, string barText) = (ValueTuple<bool, string>)e.UserState;
            progressBar.IsIndeterminate = makeProgressBarIndeterminate;
            progressBarLabel.Text = barText;

            int progress = e.ProgressPercentage;
            progressBar.Value = progress;
        }

        //worker on complete method to updating vendor products (disables loading bar, populates listboxes)
        private void UpdateVendorProductsWorkerOnComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBarLabel.Text = "";
            RefreshButton.IsEnabled = true;
            progressBar.Value = 0;
            PopulateChangeListBoxes(e.Result);
        }
    }
}
