using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules {
    static class PriceGenModule {

        private static Random rnd = new();
        private static double AddedPVM = 1.21;

        private static List<double> PriceSufixesList = new List<double>() {
            .35,
            .39,
            .45,
            .49,
            .55,
            .59,
            .65,
            .69,
            .75,
            .79,
            .85,
            .89,
            .95,
            .99
        };

        private static List<double> PriceProfitList = new List<double>() {
            .07,
            .08,
            .09,
            .10
        };

        //generates new price with random profit adding PVM into consideration and adding random price suffix
        public static double GenNewPrice(double VendorPrice) {
            double NewPrice = 0.0;

            double PriceProfit = VendorPrice * (1 + PriceProfitList[rnd.Next(PriceProfitList.Count)]);
            double PriceProfitPVM = PriceProfit * AddedPVM;

            double Price = Math.Ceiling(PriceProfitPVM);
            NewPrice = Price + PriceSufixesList[rnd.Next(PriceSufixesList.Count)];

            return NewPrice;
        }


    }
}
