using System.Windows;
using System.Windows.Controls;
using Squirrel;

namespace Ikrito_Fulfillment_Platform.Pages
{
    public partial class SettingsPage : Page
    {
        UpdateManager uManager;

        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static SettingsPage Instance { get; private set; }
        static SettingsPage()
        {
            Instance = new SettingsPage();
        }
        private SettingsPage()
        {
            InitializeComponent();
            Loaded += SettingsPage_Loaded;
        }


        //
        // Buttons section
        //

        /// <summary>
        /// Back button returns to MainPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            MainWindow.Instance.setFrame(MainPage.Instance);
        }


        //
        // Application Update Section
        //

        /// <summary>
        /// loaded method creates update manager when page finishes loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SettingsPage_Loaded(object sender, RoutedEventArgs e)
        {
            uManager = await UpdateManager.GitHubUpdateManager(@"https://github.com/Ikrito-lt/Fulfillment_Platform");
            CurrentVersionTextBox.Text = uManager.CurrentlyInstalledVersion().ToString();
        }

        /// <summary>
        /// button that updates application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            await uManager.UpdateApp();
            MessageBox.Show("Updated succesfuly!");
        }

        /// <summary>
        /// Button that checks for updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            var updateInfo = await uManager.CheckForUpdate();
            if (updateInfo.ReleasesToApply.Count > 0)
            {
                UpdateButton.IsEnabled = true;
            }
            else
            {
                UpdateButton.IsEnabled = false;
            }
        }
    }
}
