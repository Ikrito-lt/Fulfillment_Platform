using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        // todo: memory leak proble when changing pages DONE
        // solution make pages a singelton
        private void backButton_Click(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = MainPage.Instance;
        }

        private void ViewTDBProducts_Click(object sender, RoutedEventArgs e) {

        }

        private void UpdateTDBProducts_Click(object sender, RoutedEventArgs e) {
            //init
            var TDBModule = new TDBModule();
            List<Product> TDBproducts = TDBModule.getProductsFromDB();
            var productExporter = new ProductExporter(TDBproducts);

            //running export products in background
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += productExporter.exportProducts;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();

        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            loadingBar.Value = e.ProgressPercentage;
        }
    }
}
