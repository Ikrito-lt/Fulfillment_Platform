using Ikrito_Fulfillment_Platform.Utils;
using System.Windows;
using System.Windows.Controls;

namespace Ikrito_Fulfillment_Platform {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        public static MainWindow Instance { get; private set; }
        private const string WindowTitle = "Ikrito Fulfillment Platform";

        static MainWindow() {
            Instance = new MainWindow();
        }

        private MainWindow() {
            InitializeComponent();
            setFrame(MainPage.Instance);
            test();
        }

        /// <summary>
        /// for changing window title
        /// </summary>
        /// <param name="title"></param>
        public void SetWindowTitle(string title = WindowTitle)
        {
            this.Title = title;
        }

        /// <summary>
        /// for chanign the page shown by the window
        /// </summary>
        /// <param name="page"></param>
        public void setFrame(Page page)
        {
            mainFrame.Content = page;
        }

        /// <summary>
        /// for running ode form test object
        /// </summary>
        public static void test() {
            Test t = new();
        }


    }
}
