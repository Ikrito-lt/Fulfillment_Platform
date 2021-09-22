using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules {
    class RESTClient {

        private readonly string BaseUrl;

        public RESTClient(string url) {
            BaseUrl = url;
        }

        public string ExecGet(string Endpoint, Dictionary<string, string> Params) {

            //setup
            string RequestUrl = BaseUrl + Endpoint;
            RestClient client = new(RequestUrl);
            RestRequest request = new();

            //adding params to request
            foreach (KeyValuePair<string, string> pair in Params) {
                request.AddParameter(pair.Key, pair.Value);
            }

            //executing request and checking for response
            var response = client.Execute(request);
            if (response.IsSuccessful) {

                string responseContent = response.Content;
                return responseContent;
            } else {
                return ExecGet(Endpoint, Params);
            }
        }
    }
}
