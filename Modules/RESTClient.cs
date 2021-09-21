using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules {
    class RESTClient {

        //private readonly string translateEndPoint = "https://nlp-translation.p.rapidapi.com/v1/translate";

        //public string translateText(string input) {

        //    var client = new RestClient(translateEndPoint);
        //    var request = new RestRequest();
        //    request.AddParameter("text", input);
        //    request.AddParameter("to", "lt");
        //    request.AddHeader("x-rapidapi-key", "b146e783ccmsh15dcb72a5602a39p1dd3a7jsn09c539fa9bbd");
        //    request.AddHeader("x-rapidapi-host", "nlp-translation.p.rapidapi.com");

        //    var response = client.Post(request);

        //    if (response.IsSuccessful) {
        //        var content = response.Content; // Raw content as string
        //        //Console.WriteLine(content);

        //        dynamic dynamic = JsonConvert.DeserializeObject(content);
        //        return dynamic["translated_text"]["lt"];
        //    } else {
        //        return translateText(input);
        //    }

        //}


    }
}
