﻿using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Utils;
using Ikrito_Fulfillment_Platform.ViewModels;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

            //todo: make only numeric with .
            PriceBox.Text = editableProduct.price.ToString();
            VendorPriceBox.Text = editableProduct.vendor_price.ToString();
            WeightBox.Text = editableProduct.weight.ToString();

            //todo: make only numeric
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

        //on button click method that adds linkt to product.tags
        private void AddTagButton_Click(object sender, RoutedEventArgs e) {
            string newTag = TagBox.Text;
            tagListBoxDataSource.Add(newTag);
            editableProduct.images.Add(newTag);
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

    }
}
