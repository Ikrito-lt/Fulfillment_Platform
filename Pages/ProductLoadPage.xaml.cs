using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using System;
using System.Collections.Generic;
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
    public partial class ProductLoadPage : Page {

        public static ProductLoadPage Instance { get; private set; }
        static ProductLoadPage() {
            Instance = new ProductLoadPage();
        }

        private ProductLoadPage() {
            InitializeComponent();
        }

        // todo: memory leak proble when changing pages
        // solution make pages a singelton
        private void backButton_Click(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = MainPage.Instance;
        }

        private void migrateButton_Click(object sender, RoutedEventArgs e) {
            //TDBModule tdb = new();
            //tdb.PushToDB();

            //Product product = new();
            //Debug.WriteLine(product.GetImportJsonString());

            var tdbM = new TDBModule();
            //tdbM.insertCategoriesToDB();
            //tdbM.CreateProductList();
        }
    }
}
