using Ikrito_Fulfillment_Platform.Utils;
using System.Windows;
using System.Windows.Controls;

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
            test();
        }

        public void test() {
            Test t = new();
        }

        public void setFrame(Page page) {
            mainFrame.Content = page;
        }
    }
}
