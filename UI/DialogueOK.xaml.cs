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
using System.Windows.Shapes;

namespace Ikrito_Fulfillment_Platform.Utils {
    public partial class DialogueOK : Window {

        private readonly string labelStr = "Test Question yes/no?";

        public DialogueOK(string text) {
            labelStr = text;
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            LabelText.Text = labelStr;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }
    }
}
