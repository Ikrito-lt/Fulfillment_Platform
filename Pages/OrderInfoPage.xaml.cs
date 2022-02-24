using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Ikrito_Fulfillment_Platform.Pages {
    public partial class OrderInfoPage : Page {

        private readonly Order OrderInfo;
        private readonly Page PreviousPage;

        public OrderInfoPage(Order order, Page prevPage, bool canFulfill = true) {
            InitializeComponent();
            PreviousPage = prevPage;
            OrderInfo = order;

            DataContext = OrderInfo;
            OrderProductListBox.ItemsSource = OrderInfo.line_items;

            if (!canFulfill) {
                FulfillOrderButton.Visibility = Visibility.Hidden;
            }
        }


        //
        //Page navigation section
        //

        //method that chnages page to product browse page
        private void exitPage() {
            MainWindow.Instance.mainFrame.Content = PreviousPage;
        }


        //
        // Buttons section
        //

        //back button on click
        private void BackButton_Click(object sender, RoutedEventArgs e) {
            exitPage();
        }

        //view product
        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (sender is ListBoxItem listboxItem) {
                OrderProduct orderProduct= listboxItem.Content as OrderProduct;
                FullProduct viewProduct = ProductModule.GetProduct(orderProduct.sku);
                MainWindow.Instance.mainFrame.Content = new ProductEditPage(viewProduct, this, ProductCategoryModule.Instance.CategoryKVP, true);
            }
        }

        private void FulfillOrderButton_Click(object sender, RoutedEventArgs e) {
            OrderModule.FulFillOrder(OrderInfo);

            if (PreviousPage is MainPage) {
                (PreviousPage as MainPage).LoadAllOrders();
                exitPage();
            }
        }
    }
}
