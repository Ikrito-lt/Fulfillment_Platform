using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Utils;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ikrito_Fulfillment_Platform.Pages {
    public partial class ProductEditPage : Page {

        private readonly Product editableProduct;

        //todo:something interesting with refresh and buttons
        private bool productSaved = true;

        public ObservableCollection<string> imgListBoxDataSource;
        public ICommand DeleteImageCommand { get; set; }
        public ICommand ShowImageCommand { get; set; }
        
        public ObservableCollection<string> tagListBoxDataSource;
        public ICommand DeleteTagCommand { get; set; }

        public ProductEditPage(Product product) {
            InitializeComponent();
            DataContext = this;

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

            PriceBox.Text = editableProduct.price.ToString();
            VendorPriceBox.Text = editableProduct.vendor_price.ToString();
            WeightBox.Text = editableProduct.weight.ToString();

            StockBox.Text = editableProduct.stock.ToString();
            HeightBox.Text = editableProduct.height.ToString();
            WidthBox.Text = editableProduct.width.ToString();
            LenghtBox.Text = editableProduct.lenght.ToString();

            //Image listBox init
            imgListBoxDataSource = new ObservableCollection<string>(editableProduct.images);
            ImageListBox.ItemsSource = imgListBoxDataSource;
            DeleteImageCommand = new DelegateCommand<object>(DeleteImage);
            ShowImageCommand = new DelegateCommand<object>(ShowImage);

            //adding tags
            tagListBoxDataSource = new ObservableCollection<string>(editableProduct.tags);
            TagListBox.ItemsSource = tagListBoxDataSource;
            DeleteTagCommand = new DelegateCommand<object>(DeleteTag);

            // adding on change text flip product saved bool
            TitleBox.TextChanged += SaveFlip_TextChanged;
            DescBox.TextChanged += SaveFlip_TextChanged;
            VendorBox.TextChanged += SaveFlip_TextChanged;
            BarcodeBox.TextChanged += SaveFlip_TextChanged;

            PriceBox.TextChanged += SaveFlip_TextChanged;
            VendorPriceBox.TextChanged += SaveFlip_TextChanged;
            WeightBox.TextChanged += SaveFlip_TextChanged;

            StockBox.TextChanged += SaveFlip_TextChanged;
            HeightBox.TextChanged += SaveFlip_TextChanged;
            WidthBox.TextChanged += SaveFlip_TextChanged;
            LenghtBox.TextChanged += SaveFlip_TextChanged;
        }

        private Product saveProduct() {
            Product newProduct = new();

            //adding string values
            newProduct.DBID = editableProduct.DBID;
            newProduct.title = TitleBox.Text;
            newProduct.body_html = DescBox.Text;
            newProduct.vendor = VendorBox.Text;
            newProduct.barcode = BarcodeBox.Text;
            newProduct.sku = SKUBox.Text;

            //todo:repair this
            //newProduct.product_type = ProductTypeBox.Text;

            //adding doubles
            newProduct.price = double.Parse(PriceBox.Text);
            newProduct.vendor_price = double.Parse(VendorPriceBox.Text);
            newProduct.weight = double.Parse(WeightBox.Text);

            //adding ints
            newProduct.stock = int.Parse(StockBox.Text);
            newProduct.height = int.Parse(HeightBox.Text);
            newProduct.width = int.Parse(WidthBox.Text);
            newProduct.lenght = int.Parse(LenghtBox.Text);

            //adding tad and images;
            newProduct.images = editableProduct.images;
            newProduct.tags = editableProduct.tags;

            ProductModule.SaveProductToDB(newProduct);

            return newProduct;
        }

        private void exitPage() {
            MainWindow.Instance.mainFrame.Content = ProductBrowsePage.Instance;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            saveProduct();
            productSaved = true;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {
            if (productSaved) {
                exitPage();
            } else {
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

        //on button click method that adds linkt to product.tags
        private void AddTagButton_Click(object sender, RoutedEventArgs e) {
            string newTag = TagBox.Text;
            tagListBoxDataSource.Add(newTag);
            editableProduct.tags.Add(newTag);
            TagBox.Text = null;
        }

        //method deletes image link from list box
        private void DeleteTag(object item) {
            tagListBoxDataSource.Remove(item as string);
            editableProduct.tags.Remove(item as string);
        }

        //on button click method that adds linkt to product.images
        private void AddImageButton_Click(object sender, RoutedEventArgs e) {
            string newImageLink = ImageBox.Text;
            imgListBoxDataSource.Add(newImageLink);
            editableProduct.images.Add(newImageLink);
            ImageBox.Text = null;
        }

        //method deletes image link from list box
        private void DeleteImage(object item) {
            imgListBoxDataSource.Remove(item as string);
            editableProduct.images.Remove(item as string);
        }

        //method opens image in default browser
        private void ShowImage(object item) {
            string imgLink = item as string;
            SiteNav.GoToSite(imgLink);
        }

        private void DoublePreviewTextInput(object sender, TextCompositionEventArgs e) {
            Regex DoubleRegex = new Regex("[^0-9.]+");
            e.Handled = DoubleRegex.IsMatch(e.Text);
        }

        private void IntPreviewTextInput(object sender, TextCompositionEventArgs e) {
            Regex IntRegex = new Regex("[^0-9]+");
            e.Handled = IntRegex.IsMatch(e.Text);
        }

        private void SaveFlip_TextChanged(object sender, TextChangedEventArgs e) {
            productSaved = false;
        }
    }
}
