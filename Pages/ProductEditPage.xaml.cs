using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Utils;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static Ikrito_Fulfillment_Platform.Models.FullProduct;

namespace Ikrito_Fulfillment_Platform.Pages {
    public partial class ProductEditPage : Page {

        private readonly FullProduct EditableProduct;
        private readonly Dictionary<string, string> CategoryKVP;
        private readonly Page PreviousPage;
        private readonly bool isReadOnly;

        private bool ProductChanged = false;

        //for storing product variants and statuses
        private Dictionary<string, ProductVariant> EditableVariantsKVP = new();
        private List<string> ProductStatuses = ProductStatus.GetFields();

        //for handling adding images to the product
        private ObservableCollection<string> ImgListBoxDataSource;
        public ICommand DeleteImageCommand { get; set; }
        public ICommand ShowImageCommand { get; set; }

        //faor handling adding tags to the products
        private ObservableCollection<string> TagListBoxDataSource;
        public ICommand DeleteTagCommand { get; set; }

        //Constructor
        public ProductEditPage(FullProduct product, Page prevPage, Dictionary<string, string> categoryKVP, bool readOnly = false) {
            PreviousPage = prevPage;
            isReadOnly = readOnly;

            InitializeComponent();

            //changign window title to show the SKU
            MainWindow.Instance.SetWindowTitle($"Product Edit Page ({product.SKU})");

            DataContext = this;
            EditableProduct = product;
            CategoryKVP = categoryKVP;
            ProductFieldInit();

            if (isReadOnly) {
                MakePageReadonly();
            }
        }


        /// <summary>
        /// method that makes page readonly todo:redo this
        /// </summary>
        private void MakePageReadonly() {
            DescBoxLT.IsReadOnly = true;
            DescBoxLV.IsReadOnly = true;
            DescBoxEE.IsReadOnly = true;
            DescBoxRU.IsReadOnly = true;

            TitleBoxLT.IsReadOnly = true;
            TitleBoxLV.IsReadOnly = true;
            TitleBoxEE.IsReadOnly = true;
            TitleBoxRU.IsReadOnly = true;

            VendorBox.IsReadOnly = true;
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

            //status
            ProductStatusComboBox.IsEnabled = false;

            //variants
            if (!ProductVariantComboBox.Items.IsEmpty)
            {
                ProductVariantComboBox.SelectedIndex = 0;
            }    
            ProductVariantComboBox.IsEnabled = false;
            VariantTypeBox.IsEnabled = false;
            VariantDataBox.IsEnabled = false;
            VendorStockBox.IsEnabled = false;
            OurStockBox.IsEnabled = false;
            PriceBox.IsEnabled = false;
            VendorPriceBox.IsEnabled = false;
            PermPriceCheckBox.IsEnabled = false;
            SaveVariantButton.IsEnabled = false;

            //changing buttons
            SaveButton.Visibility = Visibility.Hidden;
            EditButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// initialiazes UI with editable product data
        /// </summary>
        private void ProductFieldInit() {

            TitleBoxLT.Text = EditableProduct.TitleLT;
            TitleBoxLV.Text = EditableProduct.TitleLV;
            TitleBoxEE.Text = EditableProduct.TitleEE;
            TitleBoxRU.Text = EditableProduct.TitleRU;

            DescBoxLT.Text = EditableProduct.DescLT;
            DescBoxLV.Text = EditableProduct.DescLV;
            DescBoxEE.Text = EditableProduct.DescEE;
            DescBoxRU.Text = EditableProduct.DescRU;

            VendorBox.Text = EditableProduct.Vendor;

            AddedTimeLabel.Content = EditableProduct.GetAddedTime().ToString();
            DeliveryTimeLabel.Content = EditableProduct.DeliveryTime;

            HeightBox.Text = EditableProduct.Height.ToString();
            WeightBox.Text = EditableProduct.Weight.ToString();
            WidthBox.Text = EditableProduct.Width.ToString();
            LenghtBox.Text = EditableProduct.Lenght.ToString();
            VendorProductTypeLabel.Content = EditableProduct.ProductTypeVendor;

            //Image listBox init
            ImgListBoxDataSource = new ObservableCollection<string>(EditableProduct.Images);
            ImageListBox.ItemsSource = ImgListBoxDataSource;
            DeleteImageCommand = new DelegateCommand<object>(DeleteImage);
            ShowImageCommand = new DelegateCommand<object>(ShowImage);

            //adding tags and tag commands
            TagListBoxDataSource = new ObservableCollection<string>(EditableProduct.Tags);
            TagListBox.ItemsSource = TagListBoxDataSource;
            DeleteTagCommand = new DelegateCommand<object>(DeleteTag);

            //adding attributtes to data grid
            var productAttributesArray = from row in EditableProduct.ProductAttributtes select new { AttributeName = row.Key, AttributeValue = row.Value };
            productAttributesDG.ItemsSource = productAttributesArray.ToArray();

            //setting attributte and variant labels
            ProductAttributtesLabel.Text = $"Product Attributtes ({EditableProduct.SKU})";
            ProductVariantsLabel.Text = $"Product Variants ({EditableProduct.SKU})";

            //setting up the product status UX
            ProductStatusLabel.Text = $"Product Status ({EditableProduct.SKU})";
            ProductStatusComboBox.ItemsSource = ProductStatuses;
            ProductStatusComboBox.SelectedItem = EditableProduct.Status;
            ProductStatusComboBox.Items.Refresh();

            //setting up the variants
            InitVariants();

            // adding on change text flip product saved bool
            TitleBoxLT.TextChanged += SaveFlip_TextChanged;
            TitleBoxLV.TextChanged += SaveFlip_TextChanged;
            TitleBoxEE.TextChanged += SaveFlip_TextChanged;
            TitleBoxRU.TextChanged += SaveFlip_TextChanged;
            
            DescBoxLT.TextChanged += SaveFlip_TextChanged;
            DescBoxLV.TextChanged += SaveFlip_TextChanged;
            DescBoxEE.TextChanged += SaveFlip_TextChanged;
            DescBoxRU.TextChanged += SaveFlip_TextChanged;

            VendorBox.TextChanged += SaveFlip_TextChanged;

            WeightBox.TextChanged += SaveFlip_TextChanged;
            HeightBox.TextChanged += SaveFlip_TextChanged;
            WidthBox.TextChanged += SaveFlip_TextChanged;
            LenghtBox.TextChanged += SaveFlip_TextChanged;

            //adding category KVP to product type combobox
            ProductTypeComboBox.ItemsSource = CategoryKVP;
            ProductTypeComboBox.SelectedValue = EditableProduct.ProductTypeID;
            ProductTypeComboBox.SelectionChanged += SaveFlipComboBox_SelectionChanged;
            ProductStatusComboBox.SelectionChanged += SaveFlipComboBox_SelectionChanged;
        }

        /// <summary>
        /// method to initialize product variant UX
        /// </summary>
        private void InitVariants() {
            EditableVariantsKVP.Clear();
            foreach (var pv in EditableProduct.ProductVariants) {
                EditableVariantsKVP.Add(GetVariantComboboxItemName(pv), pv);
            }

            ProductVariantComboBox.ItemsSource = EditableVariantsKVP;
            ProductVariantComboBox.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                ComboBox comboBox = e.Source as ComboBox;
                if (comboBox.SelectedItem != null)
                {
                    KeyValuePair<string, ProductVariant> variantKVP = (KeyValuePair<string, ProductVariant>)comboBox.SelectedItem;
                    ProductVariant pv = variantKVP.Value;

                    VariantTypeBox.Text = pv.VariantType;
                    VariantDataBox.Text = pv.VariantData;
                    VendorStockBox.Text = pv.VendorStock.ToString();
                    OurStockBox.Text = pv.OurStock.ToString();
                    PriceBox.Text = pv.Price.ToString();
                    VendorPriceBox.Text = pv.PriceVendor.ToString();
                    PermPriceCheckBox.IsChecked = pv.PermPrice;
                }
                else {
                    VariantTypeBox.Text = null;
                    VariantDataBox.Text = null;
                    VendorStockBox.Text = null;
                    OurStockBox.Text = null;
                    PriceBox.Text = null;
                    VendorPriceBox.Text = null;
                    PermPriceCheckBox.IsChecked = false;
                }
            };
            //selectiong first variant
            ProductVariantComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// is used to build a name for ProductVariantCombobox item KVP
        /// </summary>
        /// <param name="pv"></param>
        /// <returns></returns>
        private string GetVariantComboboxItemName(ProductVariant pv) {
            string variantBarcode;
            if (string.IsNullOrEmpty(pv.Barcode))
            {
                variantBarcode = "No Barcode";
            }
            else
            {
                variantBarcode = pv.Barcode;
            }

            string variantType;
            if (string.IsNullOrEmpty(pv.VariantType))
            {
                variantType = "No Variant Type";
            }
            else
            {
                variantType = pv.VariantType;
            }

            string variantData;
            if (string.IsNullOrEmpty(pv.VariantData))
            {
                variantData = "No Variant Data";
            }
            else
            {
                variantData = pv.VariantData;
            }

            return $"{variantBarcode} ({variantType}: {variantData})";
        }


        /// <summary>
        /// collects data from UX and created new product object, saves it to database, and  
        /// </summary>
        /// <returns></returns>
        private FullProduct saveProduct() {
            FullProduct newProduct = new();
            //adding string values
            newProduct.TitleLT = TitleBoxLT.Text;
            newProduct.TitleLV = TitleBoxLV.Text;
            newProduct.TitleEE = TitleBoxEE.Text;
            newProduct.TitleRU = TitleBoxRU.Text;

            newProduct.DescLT = DescBoxLT.Text;
            newProduct.DescLV = DescBoxLV.Text;
            newProduct.DescEE = DescBoxEE.Text;
            newProduct.DescRU = DescBoxRU.Text;

            newProduct.Vendor = VendorBox.Text;
            newProduct.SKU = EditableProduct.SKU;

            //saving product type selection category
            newProduct.ProductTypeID = ProductTypeComboBox.SelectedValue.ToString();

            //saving product dimensions
            newProduct.Height = int.Parse(HeightBox.Text);
            newProduct.Width = int.Parse(WidthBox.Text);
            newProduct.Lenght = int.Parse(LenghtBox.Text);
            newProduct.Weight = double.Parse(WeightBox.Text);

            //adding tags and images;
            newProduct.Images = ImgListBoxDataSource.ToList();
            newProduct.Tags = TagListBoxDataSource.ToList();

            //adding vendor type and added date
            newProduct.AddedTimeStamp = EditableProduct.AddedTimeStamp;
            newProduct.ProductTypeVendor = EditableProduct.ProductTypeVendor;
            newProduct.DeliveryTime = EditableProduct.DeliveryTime;

            //saving variants and attributes
            newProduct.ProductVariants = EditableVariantsKVP.Values.ToList();
            newProduct.ProductAttributtes = EditableProduct.ProductAttributtes;

            //saving product util fields
            newProduct.Status = EditableProduct.Status;
            newProduct.ProductTypeDisplayVal = ProductCategoryModule.Instance.CategoryKVP[newProduct.ProductTypeID];

            //saving Product Status
            newProduct.Status = ProductStatusComboBox.SelectedItem as string;

            //todo: change this and add manual product status change
            ProductModule.UpdateProductToDB(newProduct, newProduct.Status);

            return newProduct;
        }


        //
        // Buttons section
        //

        /// <summary>
        /// method that saves variant into variant combobox Dictionary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveVariantButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedVariantKVP = ProductVariantComboBox.SelectedItem;
            if (selectedVariantKVP != null)
            {
                ProductVariant selectedVariant = ((KeyValuePair<string, ProductVariant>)selectedVariantKVP).Value;
                string key = ((KeyValuePair<string, ProductVariant>)selectedVariantKVP).Key;

                //saving product variant data
                selectedVariant.VariantType = VariantTypeBox.Text;
                selectedVariant.VariantData = VariantDataBox.Text;
                selectedVariant.VendorStock = int.Parse(VendorStockBox.Text);
                selectedVariant.OurStock = int.Parse(OurStockBox.Text);
                selectedVariant.Price = double.Parse(PriceBox.Text);
                selectedVariant.PriceVendor = double.Parse(VendorPriceBox.Text);
                selectedVariant.PermPrice = PermPriceCheckBox.IsChecked == true;

                //replace in combobox dictionary
                string newKey = GetVariantComboboxItemName(selectedVariant);
                EditableVariantsKVP.Remove(key);
                EditableVariantsKVP.Add(newKey, selectedVariant);
                ProductVariantComboBox.ItemsSource = EditableVariantsKVP;
                ProductVariantComboBox.Items.Refresh();
            }

            //adding animation
            SaveVariantLabel.Visibility = Visibility.Visible;
            BackgroundWorker worker = new();
            worker.DoWork += (sender, e) => {
                //waits for 3s
                System.Threading.Thread.Sleep(3000);
            };
            worker.RunWorkerCompleted += (sender, e) => {
                // background Worker for loading all product on complete
                SaveVariantLabel.Visibility = Visibility.Collapsed;
            };
            worker.RunWorkerAsync();
            
            //setting product changed flag to true
            ProductChanged = true;
        }

        /// <summary>
        /// method that chnages page to product browse page
        /// </summary>
        private void exitPage() {
            //changing window title
            MainWindow.Instance.SetWindowTitle();

            //saving product logic with  out downloading it form database
            if (ProductBrowsePage.Instance != null) {
                var browsePage = ProductBrowsePage.Instance;
                FullProduct editedProductDB = ProductModule.GetProduct(EditableProduct.SKU);
                browsePage.AllProducts[editedProductDB.SKU] = editedProductDB;
                browsePage.RefreshDataGrid();
            }

            //changing price in pigu inntegration page
            if (PreviousPage is PiguIntegrationPage) { 
                (PreviousPage as PiguIntegrationPage).AllProducts[EditableProduct.SKU] = ProductModule.GetProduct(EditableProduct.SKU);
            }

            //chnaging the page
            MainWindow.Instance.mainFrame.Content = PreviousPage;
        }

        /// <summary>
        /// save button on click method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            saveProduct();
            ProductChanged = false;
            exitPage();
        }

        /// <summary>
        /// method for editng page if it is readonly
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditButton_Click(object sender, RoutedEventArgs e) {
            if (isReadOnly) {
                MainWindow.Instance.setFrame(new ProductEditPage(EditableProduct, PreviousPage, CategoryKVP));
            }
        }

        /// <summary>
        /// back button on click method (checks if product needs saving, opens confirmation dialog box and exits)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, RoutedEventArgs e) {
            if (!ProductChanged) {
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

        /// <summary>
        /// on button click method that adds link to product.tags
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddTagButton_Click(object sender, RoutedEventArgs e) {
            string newTag = TagBox.Text;
            TagListBoxDataSource.Add(newTag);
            EditableProduct.Tags.Add(newTag);
            TagBox.Text = null;

            //setting product changed flag to true
            ProductChanged = true;
        }

        /// <summary>
        /// method deletes image link from list box (passed as a command to the button)
        /// </summary>
        /// <param name="item"></param>
        private void DeleteTag(object item) {
            TagListBoxDataSource.Remove(item as string);
            EditableProduct.Tags.Remove(item as string);

            //setting product changed flag to true
            ProductChanged = true;
        }

        /// <summary>
        /// on button click method that adds linkt to product.images
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddImageButton_Click(object sender, RoutedEventArgs e) {
            string newImageLink = ImageBox.Text;
            ImgListBoxDataSource.Add(newImageLink);
            EditableProduct.Images.Add(newImageLink);
            ImageBox.Text = null;

            //setting product changed flag to true
            ProductChanged = true;
        }

        /// <summary>
        /// method deletes image link from list box (passed as a command to the button)
        /// </summary>
        /// <param name="item"></param>
        private void DeleteImage(object item) {
            ImgListBoxDataSource.Remove(item as string);
            EditableProduct.Images.Remove(item as string);

            //setting product changed flag to true
            ProductChanged = true;
        }

        /// <summary>
        /// method opens image in default browser (passed as a command to the button)
        /// </summary>
        /// <param name="item"></param>
        private void ShowImage(object item) {
            string imgLink = item as string;
            SiteNav.GoToSite(imgLink);
        }


        //
        //section for flipping ProductSaved bool and input validation of double and int inputs
        //

        /// <summary>
        /// allows only double to be entered (numbers and .)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoublePreviewTextInput(object sender, TextCompositionEventArgs e) {
            Regex DoubleRegex = new("[^0-9.]+");
            e.Handled = DoubleRegex.IsMatch(e.Text);
        }

        /// <summary>
        /// allows only int to be entered (numbers)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IntPreviewTextInput(object sender, TextCompositionEventArgs e) {
            Regex IntRegex = new("[^0-9]+");
            e.Handled = IntRegex.IsMatch(e.Text);
        }

        /// <summary>
        /// flips ProductChanged bool to indicate that product was changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFlip_TextChanged(object sender, TextChangedEventArgs e) {
            ProductChanged = true;
        }

        /// <summary>
        /// flips ProductChanged bool to indicate that product was changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFlipComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ProductChanged = true;
        }

    }
}
