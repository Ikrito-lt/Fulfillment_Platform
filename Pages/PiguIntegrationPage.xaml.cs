using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Input;
using System;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Ikrito_Fulfillment_Platform.Pages
{
    /// <summary>
    /// Interaction logic for PiguIntegrationPage.xaml
    /// </summary>
    public partial class PiguIntegrationPage : Page
    {
        public Dictionary<string, FullProduct> AllProducts;
        private Dictionary<string, string> CategoryKVP = ProductCategoryModule.Instance.CategoryKVP;
        private List<ProductListBoxItem> OurProductsLBSource = new();
        private ObservableCollection<ProductListBoxItem> PiguProductsLBSource = new();

        //is used to track that product is selected
        private string SelectedProductSKU
        {
            get { return _SelectedProductSKU; }
            set
            {
                if (!String.IsNullOrWhiteSpace(value))
                {
                    _SelectedProductSKU = value;
                    UpdateSelectedProductUI(value);
                }
            }
        }
        private string _SelectedProductSKU;

        //a command thats passed to PiguLB item to delete it from piguLB
        public ICommand DeletePiguItemCommand { get; set; }
        class ProductListBoxItem
        {
            public string SKU { get; set; } 
            public string Title { get; set; }
            public string ProductType { get; set; }
            public bool Selected { get; set; }
        }

        ////shit that makes this a singelton
        //public static PiguIntegrationPage Instance { get; private set; }
        //static PiguIntegrationPage()
        //{
        //    Instance = new PiguIntegrationPage();
        //}

        //private constructor
        public PiguIntegrationPage(Dictionary<string, FullProduct> products)
        {
            InitializeComponent();
            AllProducts = products;
            OurProductTypeFilterCBox.ItemsSource = CategoryKVP.OrderBy(key => key.Value);
            OurProductsLB.ItemsSource = OurProductsLBSource;

            //init piguLB item elete command
            DataContext = this;
            DeletePiguItemCommand = new DelegateCommand<object>(DeletePiguItem);
        }

        //
        // Buttons Section
        //

        /// <summary>
        /// back button goes back to productBrowsePage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.setFrame(ProductBrowsePage.Instance);
        }

        private void GenerateXMLButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {

        }

        //
        // Filtering our list box section
        //

        /// <summary>
        /// On combo box selection change reload our list box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OurProductTypeFilterCBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox senderCB = (sender as ComboBox);
            if (senderCB.SelectedItem != null)
            {
                KeyValuePair<string, string> kvp = (KeyValuePair<string, string>)senderCB.SelectedItem;
                PopulateOurListBox(kvp.Key);
            }
        }

        /// <summary>
        /// reloads our products list box
        /// </summary>
        /// <param name="catID"></param>
        private void PopulateOurListBox(string catID)
        {
            OurProductsLBSource.Clear();
            var pList = AllProducts.Where(p => p.Value.ProductTypeID == catID);
            foreach ((var key, var val) in pList)
            {
                ProductListBoxItem item = new ProductListBoxItem();
                item.SKU = val.SKU;
                item.Title = val.TitleLT;
                item.ProductType = val.ProductTypeDisplayVal;
                OurProductsLBSource.Add(item);
            }
            //changing count label
            OurProductsLabel.Content = $"Our Products ({OurProductsLBSource.Count})";
            OurProductsLB.Items.Refresh();
        }


        //
        // Listboxes item transfer section
        //

        /// <summary>
        /// Transfers items from pigu ListBox to our 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void ToPiguTransferBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (ProductListBoxItem item in OurProductsLBSource.Where(x => x.Selected == true)) {
                //checking if item with this sku is in pigu list box
                if (!PiguProductsLBSource.Any(x => x.SKU == item.SKU)) { 
                    PiguProductsLBSource.Add(item);
                }
            }
            PiguProductsLB.ItemsSource = PiguProductsLBSource;
            //changing count label
            PiguProductsLabel.Content = $"Pigu Products ({PiguProductsLBSource.Count})";
            PiguProductsLB.Items.Refresh();
        }

        /// <summary>
        /// unselects all items in our product listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OurLBUnSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OurProductsLBSource.ForEach(x => x.Selected = false);
            OurProductsLB.ItemsSource = OurProductsLBSource;
            OurProductsLB.Items.Refresh();
        }

        /// <summary>
        /// this method gets passed into the PiguLB item to delete the item when pressing the delete button
        /// </summary>
        /// <param name="item"></param>
        private void DeletePiguItem(object item)
        {
            PiguProductsLBSource.Remove(item as ProductListBoxItem);
        }

        /// <summary>
        /// method that handles checkbox check by pressing enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OurProductsLB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var item = sender as ListBoxItem;
            var selectedItemIndex = OurProductsLB.SelectedIndex;
            var selectedData = item.DataContext as ProductListBoxItem;

            if (selectedData != null && e.Key == Key.Enter)
            {
                OurProductsLBSource.FirstOrDefault(x => x.SKU == selectedData.SKU).Selected ^= true;
                OurProductsLB.Items.Refresh();

                OurProductsLB.UpdateLayout(); // Pre-generates item containers 
                var newFocusTarget = OurProductsLB.ItemContainerGenerator.ContainerFromIndex(selectedItemIndex) as ListBoxItem;
                if (newFocusTarget != null)
                {
                    newFocusTarget.Focus();
                }
            }
        }

        //
        // ListBoxes Selection Changed
        //

        /// <summary>
        /// changes SelectedProductSKU
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProductsLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;
            var selectedItem = lb.SelectedItem as ProductListBoxItem;
            if (selectedItem != null) { 
                SelectedProductSKU = selectedItem.SKU;
            }
        }

        /// <summary>
        /// method that displays products information when selecting a product
        /// </summary>
        /// <param name="sku"></param>
        private void UpdateSelectedProductUI(string sku) {
            Console.WriteLine( $"Pigu Integration Page Selected Product: {sku}");
            var selectedProduct = AllProducts[sku];

            //loading attributtes
            var productAttributesArray = from row in selectedProduct.ProductAttributtes select new { AttributeName = row.Key, AttributeValue = row.Value };
            productAttributesDG.ItemsSource = productAttributesArray.ToArray();

            //loading images
            ProductImagesLabel.Content = $"Product Images ({selectedProduct.Images.Count})";
            ProductImagesListBox.ItemsSource = selectedProduct.Images;

            //init product info section
            ProductInfoLabel.Text = $"Product Info ({sku})";


        }
    }
}
