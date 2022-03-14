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

namespace Ikrito_Fulfillment_Platform.Pages
{
    /// <summary>
    /// Interaction logic for PiguIntegrationPage.xaml
    /// </summary>
    public partial class PiguIntegrationPage : Page
    {
        public static PiguIntegrationPage Instance { get; private set; }
        static PiguIntegrationPage()
        {
            Instance = new PiguIntegrationPage();
        }

        private PiguIntegrationPage()
        {
            InitializeComponent();
        }
    }
}
