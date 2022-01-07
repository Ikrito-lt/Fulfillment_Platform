using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.UI;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public List<Product> AllProducts;
        private List<Product> StatusFilteredProducts;
        private List<Product> DateFilteredFilteredProducts;
        private List<Product> TextFilteredProducts;
        private List<Product> DataGridSource;

        public List<CheckBoxListItem> StatusList;

        private readonly Lazy<Dictionary<string, string>> _LazyCategoryKVP = new(() => ProductModule.GetCategoriesDictionary());
        private Dictionary<string, string> CategoryKVP => _LazyCategoryKVP.Value;


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
                    ResetTextFilters();
                }
            }
        }

        //shit that makes this a singelton
        public static ProductBrowsePage Instance { get; private set; }
        static ProductBrowsePage()
        {
            Instance = new ProductBrowsePage();
        }

        //private constructor
        private ProductBrowsePage()
        {
            InitializeComponent();

            InitStatusListBox();
            InitDatePickers();
            LoadAllProducts();
            InitTypeComboBox();
        }


        //
        // data grid manitulation section 
        //

        //method for  loading products to datagrid
        private void LoadAllProducts()
        {
            BackgroundWorker worker = new();
            worker.WorkerReportsProgress = false;
            worker.DoWork += BGW_PreloadAllProducts;
            worker.RunWorkerCompleted += BGW_PreloadAllProductsCompleted;

            //blocking refresh button and animating loading bar
            loadingBar.IsIndeterminate = true;
            RefreshButton.IsEnabled = false;
            loadingbarLabel.Text = "Loading Products";

            worker.RunWorkerAsync();
        }

        // backgroud worker for loading all products
        private void BGW_PreloadAllProducts(object sender, DoWorkEventArgs e)
        {
            List<Product> products = ProductModule.GetAllProducts();
            _ = CategoryKVP;
            e.Result = products;
        }

        // background Worker for loading all product on complete
        private void BGW_PreloadAllProductsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //getting category display names
            var TempProductList = e.Result as List<Product>;
            foreach (var TempProduct in TempProductList)
            {
                TempProduct.ProductTypeDisplayVal = CategoryKVP[TempProduct.product_type];
            }

            //putting products in their grids
            AllProducts = TempProductList;
            StatusFilteredProducts = AllProducts.ToList();
            DateFilteredFilteredProducts = AllProducts.ToList();
            TextFilteredProducts = AllProducts.ToList();

            //init DataGrid
            productDG.ItemsSource = TextFilteredProducts;

            //init label
            ChangeCountLabel(TextFilteredProducts.Count);

            //unblocking refresh button and unanimating loading bar
            loadingbarLabel.Text = "";
            loadingBar.IsIndeterminate = false;
            RefreshButton.IsEnabled = true;
            Debug.WriteLine("BGW_PreloadAllProducts Finished");
        }

        //refresh datagrid
        public void RefreshDataGrid()
        {
            //putting products in their grids
            StatusFilteredProducts = AllProducts.ToList();
            DateFilteredFilteredProducts = AllProducts.ToList();
            TextFilteredProducts = AllProducts.ToList();

            //init DataGrid
            productDG.ItemsSource = TextFilteredProducts;

            //init label
            ChangeCountLabel(TextFilteredProducts.Count);
        }

        //changes label to reflect product count in datagrid
        private void ChangeCountLabel(int count)
        {
            productCountL.Content = "Product Count: " + count.ToString();
        }

        //refreshes Product datagrid
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ResetTextFilters();
            ResetDateFilters();
            LoadAllProducts();
        }


        //
        // change pages section
        //

        //goes back to main page
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.mainFrame.Content = MainPage.Instance;
        }

        //opens Product Edit page
        private void Row_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = sender as DataGridRow;
            Product product = row.Item as Product;
            MainWindow.Instance.mainFrame.Content = new ProductEditPage(product, this, false);
        }


        //
        //status filtering section
        //

        //init status list box
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
            CheckBox6.DataContext = StatusList[5];
        }

        //on status change filtering 
        private void FilterByStatus()
        {
            StatusFilteredProducts.Clear();
            ResetDateFilters();
            ResetTextFilters();

            foreach (CheckBoxListItem status in StatusList)
            {
                if (status.IsSelected)
                {
                    StatusFilteredProducts.AddRange(AllProducts.ToList().FindAll(x => x.status == status.Name));
                }
            }

            DateFilteredFilteredProducts = StatusFilteredProducts.ToList();
            TextFilteredProducts = DateFilteredFilteredProducts.ToList();
            DataGridSource = TextFilteredProducts.ToList();
            productDG.ItemsSource = DataGridSource;
            productDG.Items.Refresh();
            ChangeCountLabel(TextFilteredProducts.Count);
        }

        // this method fires when checkbox was clicked ie selection changed
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            FilterByStatus();
        }


        //
        //Date filtering section
        //

        //init status list box
        private void InitDatePickers()
        {
            //setting begin and end date pickers
            BeginDatePicker.DisplayDateStart = new DateTime(2021, 09, 01);
            EndDatePicker.DisplayDateStart = new DateTime(2021, 09, 01);
        }

        //on selected date change
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

            if (BeginDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue)
            {
                long beginTimeStamp = ((DateTimeOffset)BeginDatePicker.SelectedDate.Value).ToUnixTimeSeconds();
                long endTimeStamp = ((DateTimeOffset)EndDatePicker.SelectedDate.Value).ToUnixTimeSeconds();
                if (beginTimeStamp <= endTimeStamp)
                {

                    DateFilteredFilteredProducts.Clear();
                    DateFilteredFilteredProducts.AddRange(StatusFilteredProducts.FindAll(x => beginTimeStamp <= long.Parse(x.addedTimeStamp) && long.Parse(x.addedTimeStamp) <= endTimeStamp));

                    TextFilteredProducts = DateFilteredFilteredProducts.ToList();
                    DataGridSource = TextFilteredProducts.ToList();
                    productDG.ItemsSource = DataGridSource;
                    productDG.Items.Refresh();
                    ChangeCountLabel(TextFilteredProducts.Count);
                }
            }
        }

        //remove / reset date filters
        private void ResetDateFilters()
        {
            BeginDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;

        }


        //
        //Type filtering section
        //

        /// <summary>
        /// init for product type filter combo box in datagrids product type header
        /// </summary>
        private void InitTypeComboBox()
        {
            List<string> typeDisplayValList = CategoryKVP.Values.ToList();
            TypeFilterCBox.ItemsSource = typeDisplayValList;
        }

        /// <summary>
        /// On selection change for category filtering
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TypeFilterCBox_KeyUp(object sender, KeyEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (e.Key == Key.Enter)
            {
                string comboBoxSelection = comboBox.Text.ToLower();

                TextFilteredProducts = TextFilteredProducts.Where(p => p.ProductTypeDisplayVal.ToLower() == comboBoxSelection).ToList();
                ChangeCountLabel(TextFilteredProducts.Count);
                DataGridSource = TextFilteredProducts;
                productDG.ItemsSource = DataGridSource;
            }
            else if (comboBox.Text.Length == 0)
            {
                return;
            }
        }

        /// <summary>
        /// open dropdown on selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TypeFilterCBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            comboBox.IsDropDownOpen = true;
        }

        /// <summary>
        /// close dropdown on unselection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TypeFilterCBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            comboBox.IsDropDownOpen = false;
        }


        //
        // Text Filtering section
        //

        //method for filtering by vendor
        private void VendorFilterSBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                string query = textBox.Text.ToLower();

                TextFilteredProducts = TextFilteredProducts.Where(p => p.vendor.ToLower().Contains(query)).ToList();
                ChangeCountLabel(TextFilteredProducts.Count);
                DataGridSource = TextFilteredProducts;
                productDG.ItemsSource = DataGridSource;
            }
            else if (textBox.Text.Length == 0)
            {
                return;
            }
        }

        //method for filtering by sku
        private void SKUFilterSBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                string query = textBox.Text.ToLower();

                TextFilteredProducts = TextFilteredProducts.Where(p => p.sku.ToLower().Contains(query)).ToList();
                ChangeCountLabel(TextFilteredProducts.Count);
                DataGridSource = TextFilteredProducts;
                productDG.ItemsSource = DataGridSource;
            }
            else if (textBox.Text.Length == 0)
            {
                return;
            }
        }

        //method for filtering by title
        private void TitleFilterSBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (e.Key == Key.Enter)
            {
                string query = textBox.Text.ToLower();

                TextFilteredProducts = TextFilteredProducts.Where(p => p.title.ToLower().Contains(query)).ToList();
                ChangeCountLabel(TextFilteredProducts.Count);
                DataGridSource = TextFilteredProducts;
                productDG.ItemsSource = DataGridSource;
            }
            else if (textBox.Text.Length == 0)
            {
                return;
            }
        }

        //method for deleting deleting filters
        private void ResetTextFilters()
        {
            TypeFilterCBox.SelectedItem = null;
            TitleFilterSBox.Clear();
            SKUFilterSBox.Clear();
            VendorFilterSBox.Clear();

            queryLenght = 0;
            TextFilteredProducts = DateFilteredFilteredProducts.ToList();
            DataGridSource = TextFilteredProducts;
            productDG.ItemsSource = DataGridSource;
            ChangeCountLabel(DataGridSource.Count);
            productDG.Items.Refresh();
        }

        //method that is triggered when clicking remove filters button
        private void RemoveFilters_Click(object sender, RoutedEventArgs e)
        {
            ResetTextFilters();
        }

        private void BulkCategoryEditButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
