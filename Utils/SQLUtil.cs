using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Utils {
    static class SQLUtil {

        public static string SQLSafeString(string input) {
            input = input.Replace("\\", $"");
            input = input.Replace("\'", $"\\'");
            input = input.Replace("\"", $"\\\"");
            return input;
        }

    }
}
