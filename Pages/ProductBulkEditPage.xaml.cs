using Ikrito_Fulfillment_Platform.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.ComponentModel;
using System;
using Ikrito_Fulfillment_Platform.Modules;

namespace Ikrito_Fulfillment_Platform.Pages
{
    public partial class ProductBulkEditPage : Page
    {
        private readonly List<Product> Products;
        private readonly Page PreviousPage;
        private readonly Dictionary<string, string> CategoryKVP;
        private readonly List<string> PossibleTypes;
        private List<string> PossibleVendorTypes;
        private List<TypeListBoxItem> ListBoxSource;
        private bool AllTypeChangeProductsSelected = false;

        class TypeListBoxItem
        {
            public string SKU { get; set; }
            public string Title { get; set; }
            public string ProductType { get; set; }
            public string VendorProductType { get; set; }
            public bool Selected { get; set; }
        }

        public ProductBulkEditPage(List<Product> products, Dictionary<string, string> categoryKVP, Page prevPage)
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

            InitTypes();
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
            Products.ForEach(x => {
                if (!possibleVendorTypes.Contains(x.productTypeVendor)) {
                    possibleVendorTypes.Add(x.productTypeVendor);
                }});
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

            foreach (Product p in Products) {
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
                    if (p.productTypeVendor != vendorProductType) continue;
                }
                else {
                    TPlistboxitem.VendorProductType = p.productTypeVendor;
                }

                //if both checks passed, give TPlistboxitem - sku - title;
                TPlistboxitem.SKU = p.sku;
                TPlistboxitem.Title = p.title;
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
                for(int i = 0; i<ListBoxSource.Count; i++) {
                    TypeListBoxItem t = (TypeListBoxItem)ListBoxSource[i];
                    ProductModule.ChangeProductCategory(t.SKU, newTypeID);
                    
                    ListBoxSource[i].ProductType = newType;
                    (sender as BackgroundWorker).ReportProgress(i);
                }
            };

            worker.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs e) =>
            {
                BackButton.IsEnabled = true;
                loadingbarLabel.Text = "";
                loadingBar.Value = 0;
                RefreshListBox();
            };

            worker.ProgressChanged += (object sender, ProgressChangedEventArgs e) =>
            {
                int progress = e.ProgressPercentage;
                loadingBar.Value = progress;
                loadingbarLabel.Text = $"Changing Product Types ({progress}/{ListBoxSource.Count})";
            };

            //blocking pre do work
            BackButton.IsEnabled = false;
            loadingbarLabel.Text = "Changing Product Types";
            loadingBar.Maximum = ListBoxSource.Count;
            
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
            MainWindow.Instance.mainFrame.Content = PreviousPage;
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
                ListBoxSource.ForEach(x => x.Selected = true);
                ChangeTypeListBox.Items.Refresh();
            }
            else {
                SelectAllProductsButton.Content = "SelectAll";
                ListBoxSource.ForEach(x => x.Selected = false);
                ChangeTypeListBox.Items.Refresh();
            }
        }

        //button that triggers type change on press
        private void ChangeTypesButton_Click(object sender, RoutedEventArgs e)
        {
            if (NewTypeCBox.SelectedItem != null) {
                ChangeTypes(NewTypeCBox.SelectedItem.ToString());
            }
        }

        private void DeleteTypeFilterButton_Click(object sender, RoutedEventArgs e)
        {
            TypeFilterCBox.SelectedItem = null;
        }

        private void DeleteVendorTypeFilterButton_Click(object sender, RoutedEventArgs e)
        {
            VendorTypeFilterCBox.SelectedItem = null;
        }

        private void DeleteNewTypeFilterButton_Click(object sender, RoutedEventArgs e)
        {
            NewTypeCBox.SelectedItem = null;
        }
    }
}
