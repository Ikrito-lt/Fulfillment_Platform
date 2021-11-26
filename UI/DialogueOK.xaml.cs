using System.Windows;

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
