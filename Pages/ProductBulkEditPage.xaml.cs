using Ikrito_Fulfillment_Platform.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Ikrito_Fulfillment_Platform.Pages
{
    public partial class ProductBulkEditPage : Page
    {
        private readonly List<Product> Products;
        private readonly Page PreviousPage;
        private readonly Dictionary<string, string> CategoryKVP;

        public ProductBulkEditPage(List<Product> products, Dictionary<string, string> categoryKVP, Page prevPage)
        {
            InitializeComponent();
            PreviousPage = prevPage;
            Products = products;
            CategoryKVP = categoryKVP;
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
    }
}
