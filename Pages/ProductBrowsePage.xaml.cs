using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace Ikrito_Fulfillment_Platform.Pages
{
    public partial class ProductBrowsePage : Page
    {

        public Dictionary<string, FullProduct> AllProducts;
        private readonly Dictionary<string, string> CategoryKVP = ProductCategoryModule.Instance.CategoryKVP;

        //for saving product status fiter state
        List<CheckBoxListItem> StatusList;
        //for saving product type filter state
        Tuple<int?, string> productTypeFilter = new(null, null);
        //for saving dates for date filtering 
        Tuple<DateTime?, DateTime?> addedDateFilter = new(null, null);

        //for saving text value filter queries
        string skuQuery = null;
        string titleQuery = null;
        string vendorQuery = null;

        //data grid item source
        private List<FullProduct> DataGridSource;


        //shit that makes this a singelton
        public static ProductBrowsePage Instance { get; private set; }
        static ProductBrowsePage()
        {
            Instance = new ProductBrowsePage();
        }
        private ProductBrowsePage()
        {
            InitializeComponent();

            InitStatusListBox();
            InitDatePickers();
            LoadAllProducts();
        }


        //
        // data grid manitulation section 
        //
        
        /// <summary>
        /// method for  loading products to datagrid
        /// </summary>
        private void LoadAllProducts()
        {
            BackgroundWorker worker = new();
            worker.WorkerReportsProgress = false;

            worker.DoWork += (sender, e) =>
            {
                //downloading products from database
                Dictionary<string, FullProduct> products = ProductModule.GetAllProducts();
                e.Result = products;
            };

            // background Worker for loading all product on complete
            worker.RunWorkerCompleted += (sender, e) =>
            {
                //getting category display names
                var TempProductList = e.Result as Dictionary<string, FullProduct>;
                foreach ((string sku, FullProduct TempProduct) in TempProductList)
                {
                    TempProduct.ProductTypeDisplayVal = CategoryKVP[TempProduct.ProductTypeID];
                }

                //loading category tree
                CategoryTreeModule categoryTreeModule = CategoryTreeModule.Instance;

                //putting products in their grids
                AllProducts = TempProductList;
                DataGridSource = AllProducts.Values.ToList();

                //init DataGrid
                productDG.ItemsSource = DataGridSource;

                //init label
                ChangeCountLabel(DataGridSource.Count);

                //unblocking refresh button and unanimating loading bar
                loadingbarLabel.Text = "";
                loadingBar.IsIndeterminate = false;
                RefreshButton.IsEnabled = true;
                BulkCategoryEditButton.IsEnabled = true;
                PiguIntegrationButton.IsEnabled = true;
                RemoveFilters.IsEnabled = true;
                SelectCategoryButton.IsEnabled = true;
                Debug.WriteLine("BGW_PreloadAllProducts Finished");
            };

            //blocking refresh button and animating loading bar
            loadingBar.IsIndeterminate = true;
            RefreshButton.IsEnabled = false;
            loadingbarLabel.Text = "Loading Products";
            BulkCategoryEditButton.IsEnabled = false;
            PiguIntegrationButton.IsEnabled = false;
            RemoveFilters.IsEnabled = false;
            SelectCategoryButton.IsEnabled = false;

            worker.RunWorkerAsync();
        }

        /// <summary>
        /// refreshes datagrid
        /// </summary>
        public void RefreshDataGrid()
        {
            //putting products in their grids
            DataGridSource = AllProducts.Values.ToList();

            //unmarking status checkboxes and date selectors 
            StatusList.ForEach(x => x.IsSelected = true);
            CheckBox1.IsChecked = true;
            CheckBox2.IsChecked = true;
            CheckBox3.IsChecked = true;
            CheckBox4.IsChecked = true;
            CheckBox5.IsChecked = true;
            BeginDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;

            //setting category to null
            SelectCategoryButton.Content = "Select Category";
            productTypeFilter = new(null, null);

            //init DataGrid
            productDG.ItemsSource = DataGridSource;

            //init label
            ChangeCountLabel(DataGridSource.Count);
        }

        /// <summary>
        /// changes label to reflect product count in datagrid
        /// </summary>
        /// <param name="count"></param>
        private void ChangeCountLabel(int count)
        {
            productCountL.Content = "Product Count: " + count.ToString();
        }

        /// <summary>
        /// refreshes Product datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshDataGrid();
            LoadAllProducts();
        }


        //
        // change pages section
        //

        /// <summary>
        /// goes back to main page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.mainFrame.Content = MainPage.Instance;
        }

        /// <summary>
        /// opens Product Edit page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Row_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            FullProduct product = row.Item as FullProduct;
            MainWindow.Instance.mainFrame.Content = new ProductEditPage(product, this, CategoryKVP, false);
        }

        /// <summary>
        /// This method changes page displayed to BulkProduct edit Page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BulkCategoryEditButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.mainFrame.Content = new ProductBulkEditPage(AllProducts, this);
        }

        /// <summary>
        /// opens pigu integration page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenPiguIntegrationPage(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.setFrame(new PiguIntegrationPage(AllProducts, CategoryKVP));
        }


        //
        // Product filtering logic 
        //

        /// <summary>
        /// method that applies status, type, date added filters to product data grid
        /// </summary>
        private void applyProductFilters()
        {
            List<FullProduct> tempList = new();

            //filtering by the product status (first because status filter is always active)
            foreach (CheckBoxListItem status in StatusList)
            {
                if (status.IsSelected)
                {
                    tempList.AddRange(AllProducts.ToList().FindAll(x => x.Value.Status == status.Name).ToDictionary(x => x.Key, x => x.Value).Values.ToList());
                }
            }

            //filtering ty product type
            if (productTypeFilter.Item1 != null && productTypeFilter.Item2 != null)
            {
                tempList = tempList.Where(x => int.Parse(x.ProductTypeID) == productTypeFilter.Item1).ToList();
            }

            //filtering products by the added date
            if (addedDateFilter.Item1 != null && addedDateFilter.Item2 != null)
            {
                long beginTimeStamp = ((DateTimeOffset)addedDateFilter.Item1).ToUnixTimeSeconds();
                long endTimeStamp = ((DateTimeOffset)addedDateFilter.Item2).ToUnixTimeSeconds();
                if (beginTimeStamp <= endTimeStamp)
                {
                    tempList = tempList.Where((x => beginTimeStamp <= long.Parse(x.AddedTimeStamp) && long.Parse(x.AddedTimeStamp) <= endTimeStamp)).ToList();
                }
            }

            //assigning temp list to datagrid source
            DataGridSource = tempList;
            productDG.ItemsSource = DataGridSource;
            ChangeCountLabel(DataGridSource.Count);
        }

        /// <summary>
        /// method that applies sku, title, vendor filters to product data grid
        /// </summary>
        private void applyProductTextFilters()
        {
            List<FullProduct> tempList = new();
            applyProductFilters();
            tempList = DataGridSource;

            //sku filter
            if (skuQuery != null)
            {
                tempList = tempList.Where(p => p.SKU.ToLower().Contains(skuQuery)).ToList();
            }

            //title filter
            if (titleQuery != null)
            {
                tempList = tempList.Where(p => p.TitleLT.ToLower().Contains(titleQuery)).ToList();
            }

            //vendor filter
            if (vendorQuery != null)
            {
                tempList = tempList.Where(p => p.Vendor.ToLower().Contains(vendorQuery)).ToList();
            }

            DataGridSource = tempList;
            productDG.ItemsSource = DataGridSource;
            ChangeCountLabel(DataGridSource.Count);
        }

        /// <summary>
        /// method that is triggered when clicking remove filters button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveFilters_Click(object sender, RoutedEventArgs e)
        {
            //deleting text filters
            SKUFilterSBox.Clear();
            TitleFilterSBox.Clear();
            VendorFilterSBox.Clear();

            skuQuery = null;
            titleQuery = null;
            vendorQuery = null;

            //deleting typpe filters
            SelectCategoryButton.Content = "Select Category";
            productTypeFilter = new(null, null);

            //deleting added date filters
            BeginDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            addedDateFilter = new(null, null);

            //deleting product status filters
            StatusList.ForEach(x => { x.IsSelected = true; });
            CheckBox1.IsChecked = true;
            CheckBox2.IsChecked = true;
            CheckBox3.IsChecked = true;
            CheckBox4.IsChecked = true;
            CheckBox5.IsChecked = true;

            applyProductFilters();
        }


        //
        //Type filtering section (category tree)
        //

        /// <summary>
        /// button should open popup with category tree
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var categoryTreeSelector = new CategoryTreeSelectorWindow("Select Category From CategoryTree");
            if (categoryTreeSelector.ShowDialog() == true)
            {
                var selectedCategoryTuple = categoryTreeSelector.selectionResult;
                button.Content = selectedCategoryTuple.Item2;
                productTypeFilter = selectedCategoryTuple;
            }
            else
            {
                button.Content = "Select Category";
                productTypeFilter = new(null, null);
            }
            applyProductFilters();
        }


        //
        //status filtering section
        //

        /// <summary>
        /// init status list box
        /// </summary>
        private void InitStatusListBox()
        {
            //getting all possible statuses and ading them to observable collection
            StatusList = new();
            foreach (var status in ProductStatus.GetFields())
            {

                CheckBoxListItem newItem = new(status);
                StatusList.Add(newItem);
            }

            //binding each checkbox to observable collection 
            CheckBox1.DataContext = StatusList[0];
            CheckBox2.DataContext = StatusList[1];
            CheckBox3.DataContext = StatusList[2];
            CheckBox4.DataContext = StatusList[3];
            CheckBox5.DataContext = StatusList[4];
        }

        /// <summary>
        /// this method fires when checkbox was clicked ie selection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            applyProductFilters();
        }


        //
        //Date filtering section
        //

        /// <summary>
        /// init status list box
        /// </summary>
        private void InitDatePickers()
        {
            //setting begin and end date pickers
            BeginDatePicker.DisplayDateStart = new DateTime(2021, 09, 01);
            EndDatePicker.DisplayDateStart = new DateTime(2021, 09, 01);
        }

        /// <summary>
        /// on selected date change if both datepicker arent empty update filter values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            if (BeginDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
            {
                addedDateFilter = new(BeginDatePicker.SelectedDate.Value, EndDatePicker.SelectedDate.Value);
                applyProductFilters();
            }
        }


        //
        // Text Filtering section
        //

        /// <summary>
        /// method for updating vendor filter query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VendorFilterSBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length > 0 && e.Key == Key.Enter)
            {
                vendorQuery = textBox.Text.ToLower();
                applyProductTextFilters();
            }
            else if (textBox.Text.Length == 0 && e.Key == Key.Enter)
            {
                vendorQuery = null;
                applyProductTextFilters();
            }
        }

        /// <summary>
        /// method for updating sku filter query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SKUFilterSBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length > 0 && e.Key == Key.Enter)
            {
                skuQuery = textBox.Text.ToLower();
                applyProductTextFilters();
            }
            else if(textBox.Text.Length == 0 && e.Key == Key.Enter)
            {
                skuQuery = null;
                applyProductTextFilters();
            }
        }

        /// <summary>
        /// method for updating title filter query
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TitleFilterSBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length > 0 && e.Key == Key.Enter)
            {
                titleQuery = textBox.Text.ToLower();
                applyProductTextFilters();
            }
            else if (textBox.Text.Length == 0 && e.Key == Key.Enter)
            {
                titleQuery = null;
                applyProductTextFilters();
            }
        }
    }
}
