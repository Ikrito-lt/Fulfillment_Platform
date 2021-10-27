using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Utils {
    class Test {
        public Test() {
            foo();
        }

        private void foo() {
            string d = $"\"{{\\\"unit\\\": \\\"cm\\\",\\\"value\\\": {11}}}";

            string heightBody = $@"{{""metafield"": {{""value"": {d} }}}}";
            Debug.WriteLine(heightBody);
        }

        //private void foo() {
        //    string json;
        //    using (StreamReader r = new StreamReader("C:\\Users\\Luke\\Desktop\\Ikrito_Fulfillment_Platform\\Files\\metaRes.json")) {
        //        json = r.ReadToEnd();
        //    }

        //    bar(json);
        //}


        //public Dictionary<string, string> bar(string json) {

        //    Dictionary<string, string> ids = new();

        //    dynamic dJson = JsonConvert.DeserializeObject(json);
        //    var metaFields = dJson["metafields"];

        //    foreach (var field in metaFields) {
        //        string key = Convert.ToString(field["key"]);
        //        string id = Convert.ToString(field["id"]);
        //        string nameSpace = Convert.ToString(field["namespace"]);

        //        if (nameSpace == "my_fields") {
        //            if (key == "lenght" || key == "height" || key == "width") {
        //                ids.Add(key, id);
        //            }
        //        }
        //    }
        //    return ids;
        //}

    }
}
