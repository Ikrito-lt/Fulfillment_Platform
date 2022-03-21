using Ikrito_Fulfillment_Platform.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.ComponentModel;
using System;
using Ikrito_Fulfillment_Platform.Modules;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Ikrito_Fulfillment_Platform.Pages
{
    public partial class ProductBulkEditPage : Page
    {
        private readonly Dictionary<string, FullProduct> Products;
        private readonly Page PreviousPage;
        private readonly Dictionary<string, string> CategoryKVP;
        private readonly List<string> PossibleTypes;
        private List<string> PossibleVendorTypes;
        private ObservableCollection<TypeListBoxItem> ListBoxSource;
        private bool AllTypeChangeProductsSelected = false;

        public ProductBulkEditPage(Dictionary<string, FullProduct> products, Dictionary<string, string> categoryKVP, Page prevPage)
        {
            InitializeComponent();
            PreviousPage = prevPage;
            Products = products;
            CategoryKVP = categoryKVP;

            //sorting lists
            PossibleTypes = CategoryKVP.Values.ToList();
            PossibleTypes.Sort();
            PossibleVendorTypes = GetVendorTypes().ToList();
            PossibleVendorTypes.Sort();
            ListBoxSource = new();

            DataContext = this;

            InitTypes();
        }

        //
        // Classes
        //

        class TypeListBoxItem
        {
            public string SKU { get; set; }
            public string Title { get; set; }
            public string ProductType { get; set; }
            public string VendorProductType { get; set; }
            public bool Selected { get; set; }
        }

        //
        // Init section
        // 

        /// <summary>
        /// 
        /// </summary>
        private void InitTypes() {
            TypeFilterCBox.ItemsSource = PossibleTypes;
            VendorTypeFilterCBox.ItemsSource = PossibleVendorTypes;
            NewTypeCBox.ItemsSource = PossibleTypes;

            ChangeTypeListBox.ItemsSource = ListBoxSource;
        }

        /// <summary>
        /// method compiles a list of vendor types
        /// </summary>
        /// <returns>List of Possible vendor types</returns>
        private List<string> GetVendorTypes() {
            List<string> possibleVendorTypes = new();
            foreach ((string sku, FullProduct p) in Products) {
                if (!possibleVendorTypes.Contains(p.ProductTypeVendor.Trim()))
                {
                    possibleVendorTypes.Add(p.ProductTypeVendor.Trim());
                }
            }
            return possibleVendorTypes;
        }

        //
        // on change section
        //

        /// <summary>
        /// Combo box selection chnaged we are doing list box filtration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox senderCB = (sender as ComboBox);
            if (senderCB.Name == "TypeFilterCBox" || senderCB.Name == "VendorTypeFilterCBox")
            {
                RefreshListBox();
            }
        }

        private void RefreshListBox() {
             string productType = string.Empty;
            string vendorProductType = string.Empty;
            if (TypeFilterCBox.SelectedItem != null) productType = TypeFilterCBox.SelectedItem.ToString();
            if (VendorTypeFilterCBox.SelectedItem != null) vendorProductType = VendorTypeFilterCBox.SelectedItem.ToString();

            ListBoxSource.Clear();

            foreach ((string sku, FullProduct p) in Products) {
                var TPlistboxitem = new TypeListBoxItem();
                TPlistboxitem.VendorProductType = vendorProductType;
                TPlistboxitem.ProductType = productType;

                //check if need to check product type, if check fails loop continues
                bool checkPT = productType == string.Empty ? false : true;
                if (checkPT)
                {
                    if (p.ProductTypeDisplayVal != productType) continue;
                }
                else {
                    TPlistboxitem.ProductType = p.ProductTypeDisplayVal;
                }

                //check if need to check vendor product type, if check fails loop continues
                bool checkVPT = TPlistboxitem.VendorProductType == string.Empty ? false : true;
                if (checkVPT)
                {
                    if (p.ProductTypeVendor.Trim() != vendorProductType) continue;
                }
                else {
                    TPlistboxitem.VendorProductType = p.ProductTypeVendor;
                }

                //if both checks passed, give TPlistboxitem - sku - title;
                TPlistboxitem.SKU = p.SKU;
                TPlistboxitem.Title = p.TitleLT;
                TPlistboxitem.Selected = false;

                //adding it to list box source
                ListBoxSource.Add(TPlistboxitem);
            }

            ChangeTypeListBox.Items.Refresh();
        }


        //
        // change products types logic section
        //

        //method for  loading products to datagrid
        private void ChangeTypes(string newType)
        {
            BackgroundWorker worker = new();
            worker.WorkerReportsProgress = true;

            worker.DoWork += (object sender, DoWorkEventArgs e) =>
            {
                string newTypeID = CategoryKVP.FirstOrDefault(x => x.Value == newType).Key;
                Dictionary<string, TypeListBoxItem> changeList = ListBoxSource.Where(x => x.Selected == true).ToDictionary(v => v.SKU, v => v);
                var i = 1;
                var changesCount = changeList.Count;
                foreach ((var sku, var t) in changeList) {
                    ProductCategoryModule.ChangeProductCategory(sku, newTypeID);

                    //changing category in products
                    Products[sku].ProductTypeID = newTypeID;
                    Products[sku].ProductTypeDisplayVal = newType;

                    //changing category in listbox
                    ListBoxSource.Remove(t);
                    (sender as BackgroundWorker).ReportProgress(i, changesCount);
                    i++;                
                }
            };

            worker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
            {
                BackButton.IsEnabled = true;
                loadingbarLabel.Text = "";
                loadingBar.Value = 0;
                ChangeTypeListBox.Items.Refresh();
            };

            worker.ProgressChanged += (object sender, ProgressChangedEventArgs e) =>
            {
                int progress = e.ProgressPercentage;
                int max = (int)e.UserState;
                loadingBar.Value = progress;
                loadingBar.Maximum = max;
                loadingbarLabel.Text = $"Changing Product Types ({progress}/{max})";
            };

            //blocking pre do work
            BackButton.IsEnabled = false;
            loadingbarLabel.Text = "Changing Product Types";
            
            worker.RunWorkerAsync();
        }

       
        //
        // Buttons section
        //

        /// <summary>
        /// Button to go back
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ProductBrowsePage.Instance.AllProducts = Products;
            ProductBrowsePage.Instance.RefreshDataGrid();
            MainWindow.Instance.mainFrame.Content = ProductBrowsePage.Instance;
        }

        /// <summary>
        /// select and unselect all typechange products in a listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAllProductsButton_Click(object sender, RoutedEventArgs e)
        {
            AllTypeChangeProductsSelected = !AllTypeChangeProductsSelected;
            if (AllTypeChangeProductsSelected)
            {
                SelectAllProductsButton.Content = "Unselect all";
                foreach (var item in ListBoxSource) {
                    item.Selected = true;
                }
                ChangeTypeListBox.Items.Refresh();
            }
            else {
                SelectAllProductsButton.Content = "SelectAll";
                foreach (var item in ListBoxSource)
                {
                    item.Selected = false;
                }
                ChangeTypeListBox.Items.Refresh();
            }
        }

        /// <summary>
        /// /button that changes types 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeTypesButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewTypeCBox.SelectedItem != null) {
                ChangeTypes(NewTypeCBox.SelectedItem.ToString());
            }
        }

        /// <summary>
        /// deletes all product filters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteTypeFilterButton_Click(object sender, RoutedEventArgs e)
        {
            TypeFilterCBox.SelectedItem = null;
        }

        /// <summary>
        /// button that unselects vendortypeCboox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteVendorTypeFilterButton_Click(object sender, RoutedEventArgs e)
        {
            VendorTypeFilterCBox.SelectedItem = null;
        }

        /// <summary>
        ///  button that unselects newtype CBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteNewTypeFilterButton_Click(object sender, RoutedEventArgs e)
        {
            NewTypeCBox.SelectedItem = null;
        }

        /// <summary>
        /// Refreshes Product attribute dataGrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeTypeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            TypeListBoxItem selectedItem = listBox.SelectedItem as TypeListBoxItem;
            if (selectedItem != null)
            {
                //loading attributtes
                string selectedSKU = selectedItem.SKU;
                FullProduct selectedProduct = Products[selectedSKU];

                var productAttributesArray = from row in selectedProduct.ProductAttributtes select new { AttributeName = row.Key, AttributeValue = row.Value };
                productAttributesDG.ItemsSource = productAttributesArray.ToArray();

                //loading images
                ProductImagesLabel.Text = $"Product Images ({selectedProduct.Images.Count})";
                ProductImagesListBox.ItemsSource = selectedProduct.Images;
            }
        }

        /// <summary>
        /// for checking listbox checkboxes by pressing enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxItem_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var item = sender as ListBoxItem;
            var selectedItemIndex = ChangeTypeListBox.SelectedIndex;
            var selectedData = item.DataContext as TypeListBoxItem;
            
            if (selectedData != null && e.Key == Key.Enter)
            {
                ListBoxSource.FirstOrDefault(x => x.SKU == selectedData.SKU).Selected ^= true;
                ChangeTypeListBox.Items.Refresh();

                ChangeTypeListBox.UpdateLayout(); // Pre-generates item containers 
                var newFocusTarget = ChangeTypeListBox.ItemContainerGenerator.ContainerFromIndex(selectedItemIndex) as ListBoxItem;
                if (newFocusTarget != null) {
                    newFocusTarget.Focus();
                }
            }
        }
    }
}
