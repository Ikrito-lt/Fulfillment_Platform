using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Modules.PiguIntegration;
using Ikrito_Fulfillment_Platform.Modules.PiguIntegration.Models;
using Ikrito_Fulfillment_Platform.UI;
using Ikrito_Fulfillment_Platform.Utils;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ikrito_Fulfillment_Platform.Pages
{
    /// <summary>
    /// Interaction logic for PiguIntegrationPage.xaml
    /// </summary>
    public partial class PiguIntegrationPage : Page
    {
        public Dictionary<string, FullProduct> AllProducts;
        private readonly Dictionary<string, string> CategoryKVP;

        //for saving selected current product type
        private Tuple<int?, string> currentProductType = new(null, null);

        //for selectin all items in our product listbox
        private bool ourListBoxAllItemsSelected = false;

        private Dictionary<string,PiguVariantOffer> SelectedProductVariantOffersKVP = new();

        private List<PiguProductOffer> OurProductsLBSource = new();
        private List<PiguProductOffer> PiguProductOfferLBSource = new();

        //used to determine if any productOffers have Variant Offers enabled
        private bool AnyOffersPlaced
        {
            get { return _AnyOffersPlaced; }
            set
            {
                _AnyOffersPlaced = value;
                GenerateXMLButton.IsEnabled = value;
            }
        }
        private bool _AnyOffersPlaced;

        //is used to track that product is selected
        private string SelectedProductSKU
        {
            get { return _SelectedProductSKU; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    Console.WriteLine($"Selected: {value}");
                    _SelectedProductSKU = value;

                    EditProductButton.IsEnabled = true;
                    UpdateSelectedProductUI(value);
                }
            }
        }
        private string _SelectedProductSKU;

        //a command thats passed to PiguLB item to delete it from piguLB
        public ICommand DeletePiguItemCommand { get; set; }

        //constructor
        public PiguIntegrationPage(Dictionary<string, FullProduct> products, Dictionary<string, string> categoryKVP)
        {
            InitializeComponent();
            AllProducts = products;
            OurProductsLB.ItemsSource = OurProductsLBSource;

            //assigning category kvp
            CategoryKVP = categoryKVP;

            //init piguLB item delete command
            DeletePiguItemCommand = new DelegateCommand<object>(DeletePiguItem);
            DataContext = this;

            //loading pigu product offers from database
            LoadPiguProductOffersDB();
        }


        //
        // Loading pigu product offers from database
        //

        /// <summary>
        /// method taht loads pigu product offers from database
        /// </summary>
        private void LoadPiguProductOffersDB() {
            //blocking exit button 
            BackButton.IsEnabled = false;

            //setting up loading bar
            loadingBarLabel.Text = "Loading Pigu Product Offers From DataBase";
            loadingBar.IsIndeterminate = true;

            //removing pigu products with no ofers
            PiguProductOfferLBSource.Clear();
            PiguProductOfferLB.Items.Refresh();

            //Building Xml in the back ground in background
            BackgroundWorker getPiguProductOffersWorker = new();
            getPiguProductOffersWorker.DoWork += (sender, e) => {
                var productOffers = PiguOfferAggregator.GetPiguProductOffersDB();
                e.Result = productOffers;
            };

            getPiguProductOffersWorker.RunWorkerCompleted += (sender, e) => {
                var TempProductOfferList = e.Result as List<PiguProductOffer>;
                PiguProductOfferLBSource = TempProductOfferList;
                PiguProductOfferLB.ItemsSource = PiguProductOfferLBSource;
                PiguProductOfferLB.Items.Refresh();

                PiguProductsLabel.Content = $"Pigu Products {PiguProductOfferLBSource.Count}";

                loadingBarLabel.Text = "";
                BackButton.IsEnabled = true;
                loadingBar.Value = 0;
                loadingBar.IsIndeterminate = false;
            };
            getPiguProductOffersWorker.RunWorkerAsync();
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

        /// <summary>
        /// button that generates and uploads xml with pigu product offers 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GenerateXMLButton_Click(object sender, RoutedEventArgs e)
        {
            //blocking exit button 
            BackButton.IsEnabled = false;
            //removing pigu products with no ofers
            PiguProductOfferLBSource.RemoveAll(x => x.AnyVariantOffersEnabled == false);
            PiguProductOfferLB.Items.Refresh();

            //getting data to pass to aggregator
            List<(PiguProductOffer, FullProduct)> passList = new List<(PiguProductOffer, FullProduct)>();
            foreach (var pOffer in PiguProductOfferLBSource) {
                passList.Add((pOffer, AllProducts[pOffer.SKU]));            
            }

            //Building Xml in the back ground in background
            BackgroundWorker piguOfferWorker = new();
            piguOfferWorker.WorkerReportsProgress = true;
            piguOfferWorker.DoWork += (sender, e) => PiguOfferAggregator.PostPiguOffers(passList, sender, e);
            
            piguOfferWorker.RunWorkerCompleted += (sender, e) => {
                loadingBarLabel.Text = "";
                BackButton.IsEnabled = true;
                loadingBar.Value = 0;
                loadingBar.IsIndeterminate = false;

                Dictionary<string, string> uploadRes = e.Result as Dictionary<string, string>;
                string message = $"ProductsXml: {uploadRes["productsXml"]}\nProductStockXml: {uploadRes["productStocksXml"]}";
                var DialogOKBox = new DialogueOK(message);
                DialogOKBox.ShowDialog();
            };

            piguOfferWorker.ProgressChanged += (sender, e) => {
                (bool makeProgressBarIndeterminate, string barText) = (ValueTuple<bool, string>)e.UserState;
                loadingBar.IsIndeterminate = makeProgressBarIndeterminate;
                loadingBarLabel.Text = barText;

                int progress = e.ProgressPercentage;
                loadingBar.Value = progress;
            };

            piguOfferWorker.RunWorkerAsync();
        }

        /// <summary>
        /// button that opens product edit page for selected product
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.setFrame(new ProductEditPage(AllProducts[SelectedProductSKU], this, CategoryKVP, true));
        }


        //
        // Filtering our list box section
        //

        /// <summary>
        /// method that updates currently selected product type state
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var categoryTreeSelector = new CategoryTreeSelectorWindow("Select Current Category From CategoryTree");
            if (categoryTreeSelector.ShowDialog() == true)
            {
                var selectedCategoryTuple = categoryTreeSelector.selectionResult;
                button.Content = selectedCategoryTuple.Item2;
                currentProductType = selectedCategoryTuple;
            }
            else
            {
                button.Content = "Select Current Category";
                currentProductType = new(null, null);
            }

            //populating our product list box
            PopulateOurListBox();
        }

        /// <summary>
        /// reloads our products list box using current producttyp state
        /// </summary>
        private void PopulateOurListBox()
        {
            OurProductsLBSource.Clear();

            //check if current product type is null, return
            if (currentProductType.Item1 == null || currentProductType.Item2 == null) {
                return;
            }

            //populating our product list box if current product type is null
            string currentTypeID = currentProductType.Item1.ToString();
            var pList = AllProducts.Where(p => p.Value.ProductTypeID == currentTypeID);
            foreach ((var key, var val) in pList)
            {
                PiguProductOffer item = new();
                item.SKU = key;
                item.Title = val.TitleLT;
                item.ProductTypeVal = val.ProductTypeDisplayVal;
                item.ProductTypeID = val.ProductTypeID;
                OurProductsLBSource.Add(item);
            }

            //changing count label
            OurProductsLabel.Content = $"Our Products ({OurProductsLBSource.Count})";
            OurProductsLB.Items.Refresh();
        }

        //
        // Listboxes item transfer, selection section
        //

        /// <summary>
        /// Transfers items from our product listbox to pigu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToPiguTransferBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (PiguProductOffer item in OurProductsLBSource.Where(x => x.Selected == true))
            {
                //checking if item with this sku is in pigu list box
                if (!PiguProductOfferLBSource.Any(x => x.SKU == item.SKU))
                {
                    item.PiguVariantOffers.Clear();
                    PiguProductOfferLBSource.Add(item);
                }
            }
            OurProductsLB.Items.Refresh();
            PiguProductOfferLB.ItemsSource = PiguProductOfferLBSource;
            //changing count label
            PiguProductsLabel.Content = $"Pigu Products ({PiguProductOfferLBSource.Count})";
            PiguProductOfferLB.Items.Refresh();
        }

        /// <summary>
        /// unselects all items in our product listbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OurLBSelectButton_Click(object sender, RoutedEventArgs e)
        {

            ourListBoxAllItemsSelected = !ourListBoxAllItemsSelected;
            if (ourListBoxAllItemsSelected)
            {
                OurLBSelectButton.Content = "Unselect all";
                foreach (var item in OurProductsLBSource)
                {
                    item.Selected = true;
                }
                OurProductsLB.Items.Refresh();
            }
            else
            {
                OurLBSelectButton.Content = "Select All";
                foreach (var item in OurProductsLBSource)
                {
                    item.Selected = false;
                }
                OurProductsLB.Items.Refresh();
            }
        }

        /// <summary>
        /// this method gets passed into the PiguLB item to delete the item when pressing the delete button
        /// </summary>
        /// <param name="item"></param>
        private void DeletePiguItem(object item)
        {
            PiguProductOfferLBSource.Remove(item as PiguProductOffer);
            PiguProductOfferLB.Items.Refresh();
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
            var selectedData = item.DataContext as PiguProductOffer;

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
        /// changes Selected SKU and offer info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OurProductsLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;
            var selectedItem = lb.SelectedItem as PiguProductOffer;
            if (selectedItem != null)
            {
                SelectedProductSKU = selectedItem.SKU;
                PiguProductOfferLB.SelectedItem = null;
            }
        }

        /// <summary>
        /// Changes selected SKU and offer info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PiguProductsLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lb = sender as ListBox;
            var selectedItem = lb.SelectedItem as PiguProductOffer;
            if (selectedItem != null)
            {
                SelectedProductSKU = selectedItem.SKU;
                OurProductsLB.SelectedItem = null;
            }
        }

        /// <summary>
        /// method that displays products information when selecting a product
        /// </summary>
        /// <param name="sku"></param>
        private void UpdateSelectedProductUI(string sku)
        {
            var selectedProduct = AllProducts[sku];

            //checking if generate xml button should be enabled
            //checking if there are any offers placed
            if (PiguProductOfferLBSource.All(x => x.AnyVariantOffersEnabled == false))
            {
                AnyOffersPlaced = false;
            }
            else {
                AnyOffersPlaced = true;
            }

            //loading attributtes
            var productAttributesArray = from row in selectedProduct.ProductAttributtes select new { AttributeName = row.Key, AttributeValue = row.Value };
            productAttributesDG.ItemsSource = productAttributesArray.ToArray();

            //loading images
            ProductImagesLabel.Content = $"Product Images ({selectedProduct.Images.Count})";
            ProductImagesListBox.ItemsSource = selectedProduct.Images;

            //init product info section
            //populating variant supplier code combo box
            SelectedProductVariantOffersKVP.Clear();
            
            //creating new piguvariantoffers
            foreach (var variant in selectedProduct.ProductVariants)
            {
                if (string.IsNullOrEmpty(variant.Barcode)) continue;
                string newPiguOfferVariantName = $"{variant.VariantType}: {variant.VariantData} ({variant.Barcode})";
                var newPiguSellOffer = new PiguVariantOffer(sku, variant.Barcode, newPiguOfferVariantName, variant.Price, variant.OurStock);
                SelectedProductVariantOffersKVP.Add(newPiguSellOffer.VariantName, newPiguSellOffer);
            }

            //disabling the variant offer info UI if there are no valid variants in the offer
            if (SelectedProductVariantOffersKVP.Count == 0)
            {
                //if product offer doesnt have any barcodes
                ProductInfoLabel.Text = $"Product Info ({sku}): Cant Sell, No Barcodes In Variants";
                ProductInfoLabel.Foreground = new SolidColorBrush(Colors.Red);
                ProductInfoLabel.Background = new SolidColorBrush(Colors.Black);
                ProductVariantComboBox.ItemsSource = null;

                //since there is not barcodes making info filds not editable and deleting info
                ProductVariantComboBox.IsEnabled = false;
                SavePiguSellOfferBtn.IsEnabled = false;

                OurPriceBox.IsEnabled = false;
                OurPriceBox.Text = null;
                
                DiscountPriceBox.IsEnabled = false;
                DiscountPriceBox.Text = null;

                OurStockBox.IsEnabled = false;
                OurStockBox.Text = null;

                //deleting info from them all the rest text blocks
                TitleLTLabel.Text = null;
                TitleLVLabel.Text = null;
                TitleEELabel.Text = null;
                TitleRULabel.Text = null;

                BarcodeBlock.Text = null;
                VendorStockBlock.Text = null;
                VendorPriceBlock.Text = null;
            }
            else
            {
                //if productoffer does have barcodes
                var selectedPiguProduct = PiguProductOfferLBSource.Where(x => x.SKU == SelectedProductSKU).FirstOrDefault();
                if (selectedPiguProduct != null && selectedPiguProduct.AnyVariantOffersEnabled == true) {
                    SelectedProductVariantOffersKVP.Clear();
                    foreach (var offer in selectedPiguProduct.PiguVariantOffers) {
                        string offerName = offer.IsEnabled ? $"{offer.VariantName} (Enabled)" : $"{offer.VariantName}";
                        SelectedProductVariantOffersKVP.Add(offerName, offer);
                    }
                }

                ProductInfoLabel.Text = $"Product Info ({sku})";
                ProductInfoLabel.Foreground = new SolidColorBrush(Colors.Black);
                ProductInfoLabel.Background = new SolidColorBrush(Colors.GhostWhite);
                ProductVariantComboBox.ItemsSource = SelectedProductVariantOffersKVP;
                ProductVariantComboBox.Items.Refresh();
                ProductVariantComboBox.SelectedIndex = 0;
            }
        }


        //
        // Variants (PiguSellOffers) section
        //

        /// <summary>
        /// changes displayed info when Variant supplier code combobox selection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProductVariantComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //hiding offer saved label
            SavePiguSellOfferLabel.Visibility = Visibility.Collapsed;
            var comboBox = sender as ComboBox;
            if (comboBox.SelectedItem != null)
            {
                var item  = (KeyValuePair<string, PiguVariantOffer>)comboBox.SelectedItem;
                var piguVariantOffer = item.Value;
                var product = AllProducts[piguVariantOffer.SKU];
                var pVariant = product.ProductVariants.Where(x => x.Barcode == piguVariantOffer.Barcode).FirstOrDefault();

                TitleLTLabel.Text = product.TitleLT;
                TitleLVLabel.Text = product.TitleLV;
                TitleEELabel.Text = product.TitleEE;
                TitleRULabel.Text = product.TitleRU;
                
                OurStockBox.Text = pVariant?.OurStock.ToString();
                OurPriceBox.Text = pVariant?.Price.ToString();
                DiscountPriceBox.Text = piguVariantOffer.PriceADiscount;

                BarcodeBlock.Text = piguVariantOffer.Barcode;
                VendorStockBlock.Text = pVariant?.VendorStock.ToString();
                VendorPriceBlock.Text = pVariant?.PriceVendor.ToString();

                //handles emanbling and disabling controls
                if (PiguProductOfferLBSource.Where(x => x.SKU == SelectedProductSKU).Count() > 0)
                {
                    ProductVariantComboBox.IsEnabled = true;
                    DiscountPriceBox.IsEnabled = true;
                    SavePiguSellOfferBtn.IsEnabled = true;
                    OurStockBox.IsEnabled = true;
                    OurPriceBox.IsEnabled = true;
                }
                else
                {
                    ProductVariantComboBox.IsEnabled = false;
                    DiscountPriceBox.IsEnabled = false;
                    SavePiguSellOfferBtn.IsEnabled = false;
                    OurStockBox.IsEnabled = false;
                    OurPriceBox.IsEnabled = false;
                }
            }
        }


        /// <summary>
        /// button that saves and enables pigu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SavePiguSellOfferBtn_Click(object sender, RoutedEventArgs e)
        {
            var oldVariantOfferIndex = ProductVariantComboBox.SelectedIndex;
            //editing offer info
            var offerKey = ((KeyValuePair<string, PiguVariantOffer>)ProductVariantComboBox.SelectedItem).Key;
            var offer = SelectedProductVariantOffersKVP[offerKey];
            var newOfferKey = offer.VariantName + " (Enabled)";
            offer.IsEnabled = true;

            //updating variant offer ourStock and ourPrice
            var currentOfferStock = OurStockBox.Text;
            var currentOfferPriceBDiscount = OurPriceBox.Text;
            if (currentOfferStock != offer.OurStock || currentOfferPriceBDiscount != offer.PriceBDiscount) {
                offer = updateProductStockAndPrice(offer);
            }

            //changing offer in offers kvp
            SelectedProductVariantOffersKVP.Remove(offerKey);
            SelectedProductVariantOffersKVP.Add(newOfferKey, offer);
            
            PiguProductOfferLBSource.Where((x) => x.SKU == SelectedProductSKU).FirstOrDefault().PiguVariantOffers = SelectedProductVariantOffersKVP.Values.ToList();
            PiguProductOfferLB.Items.Refresh();

            //changing selection
            ProductVariantComboBox.Items.Refresh();
            ProductVariantComboBox.SelectedIndex = oldVariantOfferIndex;
            SavePiguSellOfferLabel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// if variant offer stock or baseprice is changed this method changes it in the database and then in the offer itself
        /// </summary>
        /// <param name="offer"></param>
        /// <returns></returns>
        private PiguVariantOffer updateProductStockAndPrice(PiguVariantOffer offer) {
            var currentOfferStock = OurStockBox.Text;
            var currentOfferPriceBDiscount = OurPriceBox.Text;

            //changing stock and price in database
            string tablePrefix = offer.SKU.GetBeginingOrEmpty();

            DataBaseInterface db = new();
            var updateData = new Dictionary<string, string>
            {
                ["OurStock"] = currentOfferStock,
                ["Price"] = currentOfferPriceBDiscount
            };
            var whereUpdate = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = offer.SKU
                },
                ["Barcode"] = new Dictionary<string, string>
                {
                    ["="] = offer.Barcode
                }
            };
            db.Table($"_{tablePrefix}_Variants").Where(whereUpdate).Update(updateData);

            //changing stock and price in the offer
            offer.OurStock = currentOfferStock;
            offer.PriceBDiscount = currentOfferPriceBDiscount;

            return offer;
        }


        //
        // int and double preview text input section
        //

        /// <summary>
        /// allows only double to be entered (numbers and .)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoublePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //matching if value can be parsed as double
            var textBox = sender as TextBox;
            string updatedText = textBox.Text + e.Text;
            bool doubleParsable = !double.TryParse(updatedText, out _);

            e.Handled = doubleParsable;
        }

        /// <summary>
        /// allows only int to be entered (numbers)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IntPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            //matching if value can be parsed as int
            var textBox = sender as TextBox;
            string updatedText = textBox.Text + e.Text;
            bool intParsable = !int.TryParse(updatedText, out _);

            e.Handled = intParsable;
        }
    }
}
