using Ikrito_Fulfillment_Platform.Modules;
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

namespace Ikrito_Fulfillment_Platform {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public static MainWindow Instance { get; private set; }

        static MainWindow() {
            Instance = new MainWindow();
        }

        private MainWindow() {
            InitializeComponent();
            setFrame(MainPage.Instance);

            //test();
        }

        public void test() {
            //ProductSyncModule Sync = new();
            //Sync.ExportShopifyProducts();

        }

        public void setFrame(Page page) {
            mainFrame.Content = page;
        }
    }
}
