namespace Ikrito_Fulfillment_Platform.Utils {
    static class SiteNav {

        public static void GoToSite(string url) {
            System.Diagnostics.Process.Start("explorer.exe", url);
        }
    }
}
