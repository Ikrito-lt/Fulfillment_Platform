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
    public partial class DialogueYN : Window {

        private readonly string labelStr = "Test Question yes/no?";
        private bool answer;

        public DialogueYN(string text) {
            InitializeComponent();
            LabelText.Text = labelStr;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e) {

        }

        private void NoButton_Click(object sender, RoutedEventArgs e) {

        }
    }
}
