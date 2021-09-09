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
using Ikrito_Fulfillment_Platform.Utils;
using Ikrito_Fulfillment_Platform.Modules;
using Newtonsoft.Json;
using Ikrito_Fulfillment_Platform.Models;

namespace Ikrito_Fulfillment_Platform {
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page {

        private List<Order> newOrders = new();
        private NewOrderGetter newOrderGetter = new();

        public MainPage() {
            InitializeComponent();
            RefreshNewOrderDG();
        }

        private void UpdateNewOrderLabel(int count) { 
            newOrderCountL.Content = "Current Orders: " + count.ToString();
        } 

        private void RefreshNewOrderDG() {

            newOrderDG.ItemsSource = null;
            newOrders.Clear();
            newOrders.AddRange(JsonConvert.DeserializeObject<List<Order>>(newOrderGetter.getOrders()));

            UpdateNewOrderLabel(newOrders.Count);

            newOrderDG.ItemsSource = newOrders;

        }

        private void ShowOrderInfo(object sender, RoutedEventArgs e) {

            UpdateNewOrderLabel(2);
        }
    }
}
