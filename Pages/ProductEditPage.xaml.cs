using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Utils;
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

namespace Ikrito_Fulfillment_Platform.Pages {
    public partial class ProductEditPage : Page {

        private readonly Product editableProduct;
        private bool productSaved = false;
        private bool productNeedsSaving = false;

        public ProductEditPage(Product product) {
            editableProduct = product;
            InitializeComponent();
        }

        private void backButton_Click(object sender, RoutedEventArgs e) {
            DialogueYN dialog = new("Save product?");
            bool answer = dialog.ShowDialog() ?? false;

            if (answer) {
                saveProduct();
                exitPage();
            } else {
                exitPage();
            }
        }

        private void saveProduct() { 
        
        }

        private void exitPage() {
            MainWindow.Instance.mainFrame.Content = ProductBrowsePage.Instance;
        }
    }
}
