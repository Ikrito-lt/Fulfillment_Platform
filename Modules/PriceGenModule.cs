using System;
using System.Collections.Generic;

namespace Ikrito_Fulfillment_Platform.Modules {
    static class PriceGenModule {

        private static readonly Random rnd = new();
        private static readonly double AddedPVM = 1.21;

        private static readonly List<double> PriceSufixesList = new() {
            .05,
            .09,
            .15,
            .19,
            .25,
            .29,
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

        private static readonly List<double> PriceProfitList = new() {
            .07,
            .08,
            .09,
            .10
        };

        //generates new price with random profit adding PVM into consideration and adding random price suffix
        public static double GenNewPrice(double VendorPrice) {
            double PriceProfit = VendorPrice * (1 + PriceProfitList[rnd.Next(PriceProfitList.Count)]);
            double PriceProfitPVM = PriceProfit * AddedPVM;

            double Price = Math.Ceiling(PriceProfitPVM);
            double NewPrice = Price + PriceSufixesList[rnd.Next(PriceSufixesList.Count)];

            return NewPrice;
        }
    }
}
