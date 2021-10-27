using Ikrito_Fulfillment_Platform.Models;
using RestSharp;
using System.Collections.Generic;
using System.Threading;

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

        public IRestResponse ExecAddProd(string Endpoint, Dictionary<string, string> Params, string requestBody) {
            //setup
            string RequestUrl = BaseUrl + Endpoint;
            RestClient client = new(RequestUrl);
            RestRequest request = new(Method.POST);

            //adding params to request
            foreach (KeyValuePair<string, string> pair in Params) {
                request.AddHeader(pair.Key, pair.Value);
            }

            //adding body to request
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            //executing request and checking for response
            var response = client.Post(request);

            return response;
        }

        public IRestResponse ExecPutProd(string Endpoint, Dictionary<string, string> Params, string requestBody) {
            //setup
            string RequestUrl = BaseUrl + Endpoint;
            RestClient client = new(RequestUrl);
            RestRequest request = new(Method.PUT);

            //adding params to request
            foreach (KeyValuePair<string, string> pair in Params) {
                request.AddHeader(pair.Key, pair.Value);
            }

            //adding body to request
            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            //executing request and checking for response
            var response = client.Put(request);

            return response;

        }

        public bool ExecDeleteProd(Dictionary<string, string> Params, string shopifyID) {
            //setup
            string RequestUrl = BaseUrl + $"products/{shopifyID}.json";
            RestClient client = new(RequestUrl);
            RestRequest request = new(Method.DELETE);

            //adding params to request
            foreach (KeyValuePair<string, string> pair in Params) {
                request.AddHeader(pair.Key, pair.Value);
            }

            //executing request and checking for response
            var response = client.Delete(request);

            if (response.IsSuccessful) {
                return true;
            } else {
                return false;
            }
        }
    }
}
