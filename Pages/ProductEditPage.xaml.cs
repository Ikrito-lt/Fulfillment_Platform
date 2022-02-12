using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Utils;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ikrito_Fulfillment_Platform.Pages {
    public partial class ProductEditPage : Page {

        private readonly FullProduct EditableProduct;
        private readonly Dictionary<string, string> CategoryKVP;
        private readonly Page PreviousPage;
        private readonly bool isReadOnly;

        private bool ProductSaved = true;

        private ObservableCollection<string> ImgListBoxDataSource;
        public ICommand DeleteImageCommand { get; set; }
        public ICommand ShowImageCommand { get; set; }

        private ObservableCollection<string> TagListBoxDataSource;
        public ICommand DeleteTagCommand { get; set; }

        public ProductEditPage(FullProduct product, Page prevPage, bool readOnly = false) {
            PreviousPage = prevPage;
            isReadOnly = readOnly;

            InitializeComponent();

            DataContext = this;
            EditableProduct = product;
            CategoryKVP = ProductModule.GetCategoriesDictionary();
            ProductFieldInit();

            if (isReadOnly) {
                MakePageReadonly();
            }
        }


        //
        // making page readonly section
        //

        //method that makes page readonly
        private void MakePageReadonly() {
            DescBox.IsReadOnly = true;
            TitleBox.IsReadOnly = true;
            VendorBox.IsReadOnly = true;
            StockBox.IsReadOnly = true;
            BarcodeBox.IsReadOnly = true;
            PriceBox.IsReadOnly = true;
            VendorPriceBox.IsReadOnly = true;
            WeightBox.IsReadOnly = true;
            LenghtBox.IsReadOnly = true;
            HeightBox.IsReadOnly = true;
            WidthBox.IsReadOnly = true;
            ImageBox.IsReadOnly = true;
            TagBox.IsReadOnly = true;

            AddImageButton.IsEnabled = false;
            AddTagButton.IsEnabled = false;

            ProductTypeComboBox.IsEnabled = false;

            TagListBox.IsEnabled = false;
            ImageListBox.IsEnabled = false;

            //changing buttons
            SaveButton.Visibility = Visibility.Hidden;
            EditButton.Visibility = Visibility.Visible;
        }
        
        //
        // Field init and data save section
        //

        // initialiazes UI with editable product data
        private void ProductFieldInit() {

            TitleBox.Text = EditableProduct.title;
            DescBox.Text = EditableProduct.body_html;
            VendorBox.Text = EditableProduct.vendor;
            //todo: repair this
            //BarcodeBox.Text = EditableProduct.barcode;
            //SKUBox.Text = EditableProduct.sku;

            //PriceBox.Text = EditableProduct.price.ToString();
            //VendorPriceBox.Text = EditableProduct.vendor_price.ToString();
            //WeightBox.Text = EditableProduct.weight.ToString();

            //StockBox.Text = EditableProduct.stock.ToString();
            HeightBox.Text = EditableProduct.height.ToString();
            WidthBox.Text = EditableProduct.width.ToString();
            LenghtBox.Text = EditableProduct.lenght.ToString();
            VendorProductTypeLabel.Content = EditableProduct.productTypeVendor;

            //Image listBox init
            ImgListBoxDataSource = new ObservableCollection<string>(EditableProduct.images);
            ImageListBox.ItemsSource = ImgListBoxDataSource;
            DeleteImageCommand = new DelegateCommand<object>(DeleteImage);
            ShowImageCommand = new DelegateCommand<object>(ShowImage);

            //adding tags
            TagListBoxDataSource = new ObservableCollection<string>(EditableProduct.tags);
            TagListBox.ItemsSource = TagListBoxDataSource;
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

            //adding category KVP to product type combobox
            ProductTypeComboBox.ItemsSource = CategoryKVP;
            ProductTypeComboBox.SelectedValue = EditableProduct.productTypeID;
            ProductTypeComboBox.SelectionChanged += ProductTypeComboBox_SelectionChanged;
        }

        //saves data to new product 
        private FullProduct saveProduct() {
            FullProduct newProduct = new();

            //todo:repair this

            ////adding string values
            //newProduct.DBID = EditableProduct.DBID;
            //newProduct.title = TitleBox.Text;
            //newProduct.body_html = DescBox.Text;
            //newProduct.vendor = VendorBox.Text;
            //newProduct.barcode = BarcodeBox.Text;
            //newProduct.sku = EditableProduct.sku;

            ////saving product type selection category
            //newProduct.product_type = ProductTypeComboBox.SelectedValue.ToString();

            ////adding doubles
            //newProduct.price = double.Parse(PriceBox.Text);
            //newProduct.vendor_price = double.Parse(VendorPriceBox.Text);
            //newProduct.weight = double.Parse(WeightBox.Text);

            ////adding ints
            //newProduct.stock = int.Parse(StockBox.Text);
            newProduct.height = int.Parse(HeightBox.Text);
            newProduct.width = int.Parse(WidthBox.Text);
            newProduct.lenght = int.Parse(LenghtBox.Text);

            //adding tad and images;
            newProduct.images = EditableProduct.images;
            newProduct.tags = EditableProduct.tags;

            //adding vendor type and added date
            newProduct.addedTimeStamp = EditableProduct.addedTimeStamp;
            newProduct.productTypeVendor = EditableProduct.productTypeVendor;

            //todo: change this and add manual product status change
            ProductModule.UpdateProductToDB(newProduct, ProductStatus.Ok);

            return newProduct;
        }


        //
        // Buttons section
        //

        //method that chnages page to product browse page
        private void exitPage() {
            if (PreviousPage is ProductBrowsePage) {
                var page = PreviousPage as ProductBrowsePage;
                FullProduct editedProduct = ProductModule.GetProduct(EditableProduct.sku);

                page.AllProducts[editedProduct.sku] = editedProduct;
                page.RefreshDataGrid();
                MainWindow.Instance.mainFrame.Content = page;
            }
            else {
                MainWindow.Instance.mainFrame.Content = PreviousPage;
            }
        }

        //save button on click method
        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            saveProduct();
            ProductSaved = true;
            exitPage();
        }

        //method for editng page if it is readonly
        private void EditButton_Click(object sender, RoutedEventArgs e) {
            if (isReadOnly) {
                MainWindow.Instance.mainFrame.Content = new ProductEditPage(EditableProduct, PreviousPage);
            }
        }

        //back button on click method (checks if product needs saving, opens confirmation dialog box and exits)
        private void BackButton_Click(object sender, RoutedEventArgs e) {
            if (ProductSaved) {
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

        //on button click method that adds link to product.tags
        private void AddTagButton_Click(object sender, RoutedEventArgs e) {
            string newTag = TagBox.Text;
            TagListBoxDataSource.Add(newTag);
            EditableProduct.tags.Add(newTag);
            TagBox.Text = null;
        }

        //method deletes image link from list box (passed as a command to the button)
        private void DeleteTag(object item) {
            TagListBoxDataSource.Remove(item as string);
            EditableProduct.tags.Remove(item as string);
        }

        //on button click method that adds linkt to product.images
        private void AddImageButton_Click(object sender, RoutedEventArgs e) {
            string newImageLink = ImageBox.Text;
            ImgListBoxDataSource.Add(newImageLink);
            EditableProduct.images.Add(newImageLink);
            ImageBox.Text = null;
        }

        //method deletes image link from list box (passed as a command to the button)
        private void DeleteImage(object item) {
            ImgListBoxDataSource.Remove(item as string);
            EditableProduct.images.Remove(item as string);
        }

        //method opens image in default browser (passed as a command to the button)
        private void ShowImage(object item) {
            string imgLink = item as string;
            SiteNav.GoToSite(imgLink);
        }


        //
        //section for flipping ProductSaved bool and input validation of double and int inputs
        //

        //only double can be entered (numbers and .)
        private void DoublePreviewTextInput(object sender, TextCompositionEventArgs e) {
            Regex DoubleRegex = new("[^0-9.]+");
            e.Handled = DoubleRegex.IsMatch(e.Text);
        }
        
        //only int can be entered (numbers)
        private void IntPreviewTextInput(object sender, TextCompositionEventArgs e) {
            Regex IntRegex = new("[^0-9]+");
            e.Handled = IntRegex.IsMatch(e.Text);
        }

        private void SaveFlip_TextChanged(object sender, TextChangedEventArgs e) {
            ProductSaved = false;
        }

        private void ProductTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ProductSaved = false;
        }

    }
}
