using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Input;
using System;

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
        private List<ProductListBoxItem> PiguProductLBSource = new();

        class ProductListBoxItem
        {
            public string SKU { get; set; }
            public string Title { get; set; }
        }

        public PiguIntegrationPage(Dictionary<string, FullProduct> products)
        {
            InitializeComponent();
            AllProducts = products;
            OurProductTypeFilterCBox.ItemsSource = CategoryKVP.OrderBy(key => key.Value);
            OurProductsLB.ItemsSource = OurProductsLBSource;
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
                item.Title = val.Title;
                OurProductsLBSource.Add(item);
            }
            OurProductsLB.Items.Refresh();
        }

        //
        // Listboxes Drag and drop section
        //

        Point OurLBStartMousePos;
        Point PiguLBStartMousePos;

        /// <summary>
        /// records starting mouse position when selecting item in our LB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OurProductsLB_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OurLBStartMousePos = e.GetPosition(null);
        }

        private void OurProductsLB_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point mPos = e.GetPosition(null);

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(mPos.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(mPos.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                try
                {
                    // This gets the selected item
                    ListBoxItem selectedItem = (ListBoxItem)OurProductsLB.SelectedItem;
                    // You need to remove it before adding it to another listbox.
                    // if  you dont, it throws an error (due to referencing between 2 listboxes)
                    OurProductsLB.Items.Remove(selectedItem);

                    // The actual dragdrop thingy
                    // DragDropEffects.Copy... i dont think this matters but oh well.
                    DragDrop.DoDragDrop(this, new DataObject(DataFormats.FileDrop, selectedItem), DragDropEffects.Copy);

                    // This code will check if the listboxitem is inside a ListBox or not.
                    // This will stop the ListBoxItem you dragged from vanishing if you dont
                    // Drop it inside a listbox (drop it in the titlebar or something lol)

                    // ListBoxItems are objects obviously, and objects are passed and moved by reference.
                    // Any change to an object affects every reference. 'selectedItem' is a reference
                    // To LB2.SelectedItem, and they both will NEVER be different :)

                    if (selectedItem.Parent == null)
                    {
                        OurProductsLB.Items.Add(selectedItem);
                    }
                }
                catch { }
            }
        }

        private void OurProductsLB_Drop(object sender, DragEventArgs e)
        {
            // This casts 'e.Data.GetData()' as a ListBoxItem and if it isn't null
            // then the code will "execute" sort of. basically, listItem will always be 
            // a ListBoxItem (atleast i think it will)
            if (e.Data.GetData(DataFormats.FileDrop) is ListBoxItem listItem)
            {
                OurProductsLB.Items.Add(listItem);
            }
        }


        /// <summary>
        /// records starting mouse position when selecting item in pigu LB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PiguProductsLB_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PiguLBStartMousePos = e.GetPosition(null);
        }

        private void PiguProductsLB_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point mPos = e.GetPosition(null);

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(mPos.X) > SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(mPos.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                try
                {
                    // This gets the selected item
                    ListBoxItem selectedItem = (ListBoxItem)PiguProductsLB.SelectedItem;
                    // You need to remove it before adding it to another listbox.
                    // if  you dont, it throws an error (due to referencing between 2 listboxes)
                    PiguProductsLB.Items.Remove(selectedItem);

                    // The actual dragdrop thingy
                    // DragDropEffects.Copy... i dont think this matters but oh well.
                    DragDrop.DoDragDrop(this, new DataObject(DataFormats.FileDrop, selectedItem), DragDropEffects.Copy);

                    // This code will check if the listboxitem is inside a ListBox or not.
                    // This will stop the ListBoxItem you dragged from vanishing if you dont
                    // Drop it inside a listbox (drop it in the titlebar or something lol)

                    // ListBoxItems are objects obviously, and objects are passed and moved by reference.
                    // Any change to an object affects every reference. 'selectedItem' is a reference
                    // To LB2.SelectedItem, and they both will NEVER be different :)

                    if (selectedItem.Parent == null)
                    {
                        PiguProductsLB.Items.Add(selectedItem);
                    }
                }
                catch { }
            }
        }

        private void PiguProductsLB_Drop(object sender, DragEventArgs e)
        {
            // This casts 'e.Data.GetData()' as a ListBoxItem and if it isn't null
            // then the code will "execute" sort of. basically, listItem will always be 
            // a ListBoxItem (atleast i think it will)
            if (e.Data.GetData(DataFormats.FileDrop) is ListBoxItem listItem)
            {
                PiguProductsLB.Items.Add(listItem);
            }
        }
    }
}
