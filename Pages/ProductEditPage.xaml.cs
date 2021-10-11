using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ikrito_Fulfillment_Platform.Pages {
    public partial class ProductEditPage : Page {

        private readonly Product editableProduct;
        private bool productSaved = false;
        private bool productNeedsSaving = false;

        public ProductEditPage(Product product) {
            InitializeComponent();
            editableProduct = product;
            ProductFieldInit();
        }

        /// <summary>
        /// method to add all vals to page forms on init
        /// </summary>
        private void ProductFieldInit() {

            TitleBox.Text = editableProduct.title;
            DescBox.Text = editableProduct.body_html;
            VendorBox.Text = editableProduct.vendor;
            ProductTypeBox.Text = editableProduct.product_type;
            BarcodeBox.Text = editableProduct.barcode;
            SKUBox.Text = editableProduct.sku;

            //todo: make only numeric with .
            PriceBox.Text = editableProduct.price.ToString();
            VendorPriceBox.Text = editableProduct.vendor_price.ToString();
            WeightBox.Text = editableProduct.weight.ToString();

            //todo: make only numeric
            StockBox.Text = editableProduct.stock.ToString();
            HeightBox.Text = editableProduct.height.ToString();
            WidthBox.Text = editableProduct.width.ToString();
            LenghtBox.Text = editableProduct.lenght.ToString();

            //Adding images
            ImageListBox.ItemsSource = editableProduct.images;

            //adding tags
            TagListBox.ItemsSource = editableProduct.tags;

        }

        private void AddImageButton_Click(object sender, RoutedEventArgs e) {

        }

        private void AddTagButton_Click(object sender, RoutedEventArgs e) {

        }

        private void saveProduct() {

        }

        private void exitPage() {
            MainWindow.Instance.mainFrame.Content = ProductBrowsePage.Instance;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {

        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {
            DialogueYN dialog = new("Save product?");
            bool answer = dialog.ShowDialog() ?? false;

            if (answer) {
                saveProduct();
                exitPage();
            } else {
                exitPage();
            }
        }

    }
}
