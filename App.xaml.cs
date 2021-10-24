using Ikrito_Fulfillment_Platform.Modules;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
    }
}
