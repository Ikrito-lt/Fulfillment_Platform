using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules;
using Ikrito_Fulfillment_Platform.Utils;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Ikrito_Fulfillment_Platform.Pages {
    public partial class ProductSyncPage : Page {

        private readonly List<SyncProduct> syncProducts;

        //shit makes it a singleton
        public static ProductSyncPage Instance { get; private set; }
        
        static ProductSyncPage() {
            Instance = new ProductSyncPage();
        }

        private ProductSyncPage() {
            InitializeComponent();

            syncProducts = GetSyncProducts();
            productSyncDG.ItemsSource = syncProducts;
        }

        private static List<SyncProduct> GetSyncProducts() {
            List<SyncProduct> p = new();

            DataBaseInterface db = new();
            var result = db.Table("ProductUpdates").Get();

            foreach (var row in result.Values) {
                SyncProduct product = new();

                product.id = int.Parse(row["ID"]);
                product.productID = int.Parse(row["ProductID"]);

                product.sku = row["SKU"];
                product.status = row["Status"];
                product.lastSyncTime = row["LastSyncTime"].UnixTimeToSrt();
                product.lastUpdateTime = row["LastUpdateTime"].UnixTimeToSrt();
                p.Add(product);
            }
            return p;
        }


        private void BackButton_Click(object sender, RoutedEventArgs e) {
            MainWindow.Instance.mainFrame.Content = ProductBrowsePage.Instance;
        }





        //figure it out
        private void UpdateProducts_Click(object sender, RoutedEventArgs e) {
            //init
            var TDBModule = new TDBModule();
            List<Product> TDBproducts = Modules.TDBModule.getProductsFromDB();
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
