using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Modules.PiguIntegration;
using Ikrito_Fulfillment_Platform.Modules.PiguIntegration.Models;
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
        private Dictionary<string, string> CategoryKVP = ProductCategoryModule.Instance.CategoryKVP;
        private List<PiguProductOffer> OurProductsLBSource = new();
        private List<PiguProductOffer> PiguProductOfferLBSource = new();
        private Dictionary<string,PiguVariantOffer> SelectedProductVariantOffersKVP = new();

        //used to detemrine if any productOffers have Variant Offers enabled
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

        //private constructor
        public PiguIntegrationPage(Dictionary<string, FullProduct> products)
        {
            InitializeComponent();
            AllProducts = products;
            OurProductTypeFilterCBox.ItemsSource = CategoryKVP.OrderBy(key => key.Value);
            OurProductsLB.ItemsSource = OurProductsLBSource;

            //init piguLB item elete command
            DeletePiguItemCommand = new DelegateCommand<object>(DeletePiguItem);
            DataContext = this;

            //loaign pigu product from database
            LoadPiguProductOffersDB();

        }


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

        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Instance.setFrame(new ProductEditPage(AllProducts[SelectedProductSKU], this, CategoryKVP, true));
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
                PiguProductOffer item = new PiguProductOffer();
                item.SKU = val.SKU;
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
        // Listboxes item transfer section
        //

        /// <summary>
        /// Transfers items from pigu ListBox to our 
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
        /// changes SelectedProductSKU
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
            
            //creatign new piguvariantoffers
            foreach (var variant in selectedProduct.ProductVariants)
            {
                if (string.IsNullOrEmpty(variant.Barcode)) continue;
                string newPiguOfferVariantName = $"{variant.VariantType}: {variant.VariantData}";
                var newPiguSellOffer = new PiguVariantOffer(sku, variant.Barcode, newPiguOfferVariantName, variant.Price, variant.OurStock);
                SelectedProductVariantOffersKVP.Add(newPiguSellOffer.VariantName, newPiguSellOffer);
            }

            if (SelectedProductVariantOffersKVP.Count == 0)
            {
                //if product offer doesnt have any barcodes
                ProductInfoLabel.Text = $"Product Info ({sku}): Cant Sell, No Barcodes In Variants";
                ProductInfoLabel.Foreground = new SolidColorBrush(Colors.Red);
                ProductInfoLabel.Background = new SolidColorBrush(Colors.Black);
                ProductVariantComboBox.ItemsSource = null;

                //since there is not barcodes making info filds not editable and deleting info
                ProductVariantComboBox.IsEnabled = false;
                OurPriceBox.IsEnabled = false;
                DiscountPriceBox.IsEnabled = false;
                SavePiguSellOfferBtn.IsEnabled = false;
                //deleting info from them
                TitleLTLabel.Text = null;
                TitleLVLabel.Text = null;
                TitleEELabel.Text = null;
                TitleRULabel.Text = null;

                BarcodeBlock.Text = null;
                VendorStockBlock.Text = null;
                OurStockBlock.Text = null;
                VendorPriceBlock.Text = null;
                OurPriceBox.Text = null;
                ProfitPerUnitSoldBlock.Text = null;
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

                BarcodeBlock.Text = piguVariantOffer.Barcode;
                VendorStockBlock.Text = pVariant?.VendorStock.ToString();
                OurStockBlock.Text = pVariant?.OurStock.ToString();
                VendorPriceBlock.Text = pVariant?.PriceVendor.ToString();
                OurPriceBox.Text = pVariant?.Price.ToString();
                DiscountPriceBox.Text = piguVariantOffer.PriceADiscount;

                if (piguVariantOffer.IsEnabled && piguVariantOffer.PriceADiscount != "0")
                {
                    ProfitPerUnitSoldBlock.Text = Math.Round(piguVariantOffer.PriceADiscoutDouble - pVariant.PriceVendor, 2).ToString();
                }
                else {
                    
                    ProfitPerUnitSoldBlock.Text = Math.Round(pVariant.Price - pVariant.PriceVendor, 2).ToString();
                }
                

                //handles emanbling and disabling controls
                if (PiguProductOfferLBSource.Where(x => x.SKU == SelectedProductSKU).Count() > 0)
                {
                    ProductVariantComboBox.IsEnabled = true;
                    DiscountPriceBox.IsEnabled = true;
                    SavePiguSellOfferBtn.IsEnabled = true;
                }
                else
                {
                    ProductVariantComboBox.IsEnabled = false;
                    DiscountPriceBox.IsEnabled = false;
                    SavePiguSellOfferBtn.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// allows only double to be entered (numbers and .)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoublePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex DoubleRegex = new(@"[^0-9.]+");
            e.Handled = DoubleRegex.IsMatch(e.Text);
        }

        /// <summary>
        /// button that saves and enables pigu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SavePiguSellOfferBtn_Click(object sender, RoutedEventArgs e)
        {
            var oldVariantOfferIndex = ProductVariantComboBox.SelectedIndex;
            //edititng offer info
            var offerKey = ((KeyValuePair<string, PiguVariantOffer>)ProductVariantComboBox.SelectedItem).Key;
            var offer = SelectedProductVariantOffersKVP[offerKey];
            var newOfferKey = offer.VariantName + " (Enabled)";
            offer.IsEnabled = true;

            //changing offer in offers kvp
            SelectedProductVariantOffersKVP.Remove(offerKey);
            SelectedProductVariantOffersKVP.Add(newOfferKey, offer);
            
            PiguProductOfferLBSource.Where((x) => x.SKU == SelectedProductSKU).FirstOrDefault().PiguVariantOffers = SelectedProductVariantOffersKVP.Values.ToList();
            PiguProductOfferLB.Items.Refresh();

            //changign selection
            ProductVariantComboBox.Items.Refresh();
            ProductVariantComboBox.SelectedIndex = oldVariantOfferIndex;
            SavePiguSellOfferLabel.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// calculates the PPUS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DiscountPriceBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var s = (sender as TextBox);
            var text = s.Text;
            double discountPrice;

            if(ProductVariantComboBox.SelectedItem != null)
            {
                if (Double.TryParse(text, out discountPrice))
                {
                    //calc ppus
                    if (discountPrice <= 0) {
                        DiscountPriceBox.Background = new SolidColorBrush(Colors.White);
                        var offer = ((KeyValuePair<string, PiguVariantOffer>)ProductVariantComboBox.SelectedItem).Value;
                        offer.PriceADiscount = "0";

                        var pVariant = AllProducts[offer.SKU].ProductVariants.Where(x => x.Barcode == offer.Barcode).FirstOrDefault();
                        ProfitPerUnitSoldBlock.Text = Math.Round(pVariant.Price - pVariant.PriceVendor, 2).ToString();

                    }
                    else
                    {
                        discountPrice = Math.Round(discountPrice, 2);
                        DiscountPriceBox.Background = new SolidColorBrush(Colors.White);
                        var offer = ((KeyValuePair<string, PiguVariantOffer>)ProductVariantComboBox.SelectedItem).Value;
                        offer.PriceADiscount = text;

                        var vendorPrice = AllProducts[offer.SKU].ProductVariants.Where(x => x.Barcode == offer.Barcode).FirstOrDefault().PriceVendor;

                        ProfitPerUnitSoldBlock.Text = Math.Round(discountPrice - vendorPrice, 2).ToString();
                    }
                    
                }
                else
                {
                    DiscountPriceBox.Background = new SolidColorBrush(Colors.Red);
                }
            }
        }
    }
}
