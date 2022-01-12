using Ikrito_Fulfillment_Platform.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

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
            PossibleTypes = CategoryKVP.Values.ToList();
            PossibleVendorTypes = GetVendorTypes();
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

            //var a = new TypeListBoxItem();
            //a.SKU = "aasdfasdfasdfasdfasdfasdfasdfsadfs";
            //a.Title = "aasdfsadfasdfasdfasdfasdfasdfsadfasdfasdfs";
            //a.ProductType = " asdfasdfasdfsadfasdfsadfsa";
            //a.VendorProductType = " asdasdasdasdasdasd";

            //ListBoxSource.Add(a);
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
            string vendorProductType = VendorTypeFilterCBox.SelectedItem.ToString();

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

        private void ChangeTypesButton_Click(object sender, RoutedEventArgs e)
        {

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
