﻿using Ikrito_Fulfillment_Platform.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ikrito_Fulfillment_Platform.Pages {
    public partial class OrderInfoPage : Page {

        private readonly Order OrderInfo;
        private readonly Page PreviousPage;

        public OrderInfoPage(Order order, Page prevPage) {
            InitializeComponent();
            PreviousPage = prevPage;
            OrderInfo = order;

            DataContext = OrderInfo;
            OrderProductListBox.ItemsSource = OrderInfo.line_items;
        }


        //
        //Page navigation section
        //

        //back button on click
        private void BackButton_Click(object sender, RoutedEventArgs e) {
            exitPage();
        }


        //
        // Buttons section
        //

        //method that chnages page to product browse page
        private void exitPage() {
            MainWindow.Instance.mainFrame.Content = PreviousPage;
        }

        //view product
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

        }
    }
}
