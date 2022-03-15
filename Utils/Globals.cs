using System;
using System.Collections.Generic;
using System.Text;

namespace Ikrito_Fulfillment_Platform.Utils {
    static class Globals {
        // Shopify app creds
        private const string shopifyAPIKey = "40a65778e811be7da9d06d9dcc6d7e8e";
        private static readonly string shopifyAPIPass = "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";

        public static string getBase64ShopifyCreds() {
            string preEncodeCreds = shopifyAPIKey + ":" + shopifyAPIPass;
            byte[] textBytes = Encoding.UTF8.GetBytes(preEncodeCreds);
            string encodedCreds = Convert.ToBase64String(textBytes);

            return "Basic " + encodedCreds;
        }

        //TDB Creds
        public static readonly Dictionary<string, string> _TDBAPIParams = new() {
            ["orgnum"] = "268230",
            ["username"] = "PREKES",
            ["pwd"] = "Welcome.123",
            ["ean"] = "y"
        };

        //Kotryna Group Creds
        public const string KGApi = "https://ws.kotrynagroup.com/api/";
        public const string KGApiKey = "ef364d8416ccbaf9f1ad1db3835e40c7064dd2b9";

        //Pretendentas API Creds
        public const string PDApiKey = "390ad56b495968e4679e2be66ed507ad";

        // AWS mySQL DB creds (old)
        public const string oldDBHostname = "ikrito-db-v2.c175yaycfw7i.eu-central-1.rds.amazonaws.com";
        public const string oldDBHostnamePort = "3306";
        public const string oldDBUsername = "admin";
        public const string oldDBPassword = "p3gD2Z5fbztX8Uh";
        public const string oldDefaultDB = "main";

        // AWS mySQL DB creds (main)
        public const string DBServer = "ikrito-db-v3.c175yaycfw7i.eu-central-1.rds.amazonaws.com";
        public const string DBServerPort = "3306";
        public const string DBServerUsername = "admin";
        public const string DBServerPassword = "p3gD2Z5fbztX8Uh";
        public const string DefaultDB = "main";

        // AWS mySQL DB creds (webShop)
        public const string DBServerWS = "ikrito-db-v3.c175yaycfw7i.eu-central-1.rds.amazonaws.com";
        public const string DBServerPortWS = "3306";
        public const string DBServerUsernameWS = "webShop";
        public const string DBServerPasswordWS = "HUVVU6UgKaEdvZ";


        //shopify orders endpoint
        public const string getOrdersEndPoint = "https://real-europe-corp.myshopify.com/admin/api/2021-07/orders.json";
    }
}
