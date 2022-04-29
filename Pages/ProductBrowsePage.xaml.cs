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
        private Dictionary<string, string> CategoryKVP = ProductCategoryModule.Instance.CategoryKVP;

        //for saving product type filter state
        (int?, string?) productTypeFilter = (null, null);
        //for saving dates for date filtering 
        (DateTime?, DateTime?) addedDateFilter = (null, null);
        //for saving product status fiter state
        List<CheckBoxListItem> StatusList;

        //for saving text value filter queries
        string? skuQuery = null;
        string? titleQuery = null;
        string? vendorQuery = null;

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

            worker.DoWork += (sender, e) => {
                //downloading products from database
                Dictionary<string, FullProduct> products = ProductModule.GetAllProducts();
                e.Result = products;
            };

            // background Worker for loading all product on complete
            worker.RunWorkerCompleted += (sender, e) => {
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
                Debug.WriteLine("BGW_PreloadAllProducts Finished");
                BulkCategoryEditButton.IsEnabled = true;
                PiguIntegrationButton.IsEnabled = true;
                RemoveFilters.IsEnabled = true;
                SelectCategoryButton.IsEnabled = true;
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
            productTypeFilter = (null, null);

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
            MainWindow.Instance.mainFrame.Content = new ProductBulkEditPage(AllProducts, CategoryKVP, this);
        }

        /// <summary>
        /// opens pigu integration page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenPiguIntegrationPage(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.setFrame(new PiguIntegrationPage(AllProducts));
        }


        //
        // Product filtering logic 
        //

        /// <summary>
        /// mehod that applies all filters to product data grid
        /// </summary>
        private void applyProductFilters() { 

            List<FullProduct> tempList = new List<FullProduct>();

            //filtering ty product type
            if (productTypeFilter.Item1 != null && productTypeFilter.Item2 != null)
            {
                tempList = AllProducts.Values.ToList().Where(x => int.Parse(x.ProductTypeID) == productTypeFilter.Item1).ToList();
            }
            else { 
                tempList = AllProducts.Values.ToList();
            }

            //assigning temp list to datagrid source
            DataGridSource = tempList;
            productDG.ItemsSource = DataGridSource;
            ChangeCountLabel(DataGridSource.Count);
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
            var categoryTreeSelector = new CategoryTreeSelectorWindow();
            if (categoryTreeSelector.ShowDialog() == true)
            {
                (int, string) selectedCategory = categoryTreeSelector.selectionResult;
                button.Content = selectedCategory.Item2;
                productTypeFilter = selectedCategory;
            }
            else
            {
                button.Content = "Select Category";
                productTypeFilter = (null, null);
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
                addedDateFilter = (BeginDatePicker.SelectedDate.Value, EndDatePicker.SelectedDate.Value);
                applyProductFilters();

                //long beginTimeStamp = ((DateTimeOffset)BeginDatePicker.SelectedDate.Value).ToUnixTimeSeconds();
                //long endTimeStamp = ((DateTimeOffset)EndDatePicker.SelectedDate.Value).ToUnixTimeSeconds();
                //if (beginTimeStamp <= endTimeStamp)
                //{

                //    DateFilteredFilteredProducts.Clear();
                //    DateFilteredFilteredProducts.AddRange(StatusFilteredProducts.FindAll(x => beginTimeStamp <= long.Parse(x.AddedTimeStamp) && long.Parse(x.AddedTimeStamp) <= endTimeStamp));

                //    TextFilteredProducts = DateFilteredFilteredProducts.ToList();
                //    DataGridSource = TextFilteredProducts.ToList();
                //    productDG.ItemsSource = DataGridSource;
                //    productDG.Items.Refresh();
                //    ChangeCountLabel(TextFilteredProducts.Count);
                //}
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
            if (e.Key == Key.Enter)
            {
                vendorQuery = textBox.Text.ToLower();
                applyProductFilters();

                //TextFilteredProducts = TextFilteredProducts.Where(p => p.Vendor.ToLower().Contains(query)).ToList();
                //ChangeCountLabel(TextFilteredProducts.Count);
                //DataGridSource = TextFilteredProducts;
                //productDG.ItemsSource = DataGridSource;
            }
            else if (textBox.Text.Length == 0)
            {
                vendorQuery = null;
                applyProductFilters();
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
            if (e.Key == Key.Enter)
            {
                skuQuery = textBox.Text.ToLower();
                applyProductFilters();
                //TextFilteredProducts = TextFilteredProducts.Where(p => p.SKU.ToLower().Contains(query)).ToList();
                //ChangeCountLabel(TextFilteredProducts.Count);
                //DataGridSource = TextFilteredProducts;
                //productDG.ItemsSource = DataGridSource;
            }
            else if (textBox.Text.Length == 0)
            {
                skuQuery = null;
                applyProductFilters();
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
            if (e.Key == Key.Enter)
            {
                titleQuery = textBox.Text.ToLower();
                applyProductFilters();
                //TextFilteredProducts = TextFilteredProducts.Where(p => p.TitleLT.ToLower().Contains(query)).ToList();
                //ChangeCountLabel(TextFilteredProducts.Count);
                //DataGridSource = TextFilteredProducts;
                //productDG.ItemsSource = DataGridSource;
            }
            else if (textBox.Text.Length == 0)
            {
                titleQuery = null;
                applyProductFilters();
            }
        }

        /// <summary>
        /// method that is triggered when clicking remove filters button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveFilters_Click(object sender, RoutedEventArgs e)
        {
            TitleFilterSBox.Clear();
            SKUFilterSBox.Clear();
            VendorFilterSBox.Clear();
        }
    }
}
