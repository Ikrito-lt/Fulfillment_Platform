using System.Windows;

namespace Ikrito_Fulfillment_Platform {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {

        public App() {
            Startup += App_Startup;
        }

        void App_Startup(object sender, StartupEventArgs e) {
            Ikrito_Fulfillment_Platform.MainWindow.Instance.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) {
            MessageBox.Show("An exception just occurred:\n" + e.Exception.Message + "\n\nSend screenshot you know where.", "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
