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

        public string ExecPostProd(string Endpoint, Dictionary<string, string> Params, string requestBody, SyncProduct sync) {
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

            if (response.IsSuccessful) {

                string responseContent = response.Content;
                return responseContent;
            } else {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                    Thread.Sleep(5000);

                    bool deleted = ExecDeleteProd(Params, sync.shopifyID);
                    if (deleted == false) {
                        return "503 and DeleteFail";
                    }

                    return ExecPostProd(Endpoint, Params, requestBody, sync);
                } else {
                    throw response.ErrorException;
                }
            }
        }

        public bool ExecPostProdBool(string Endpoint, Dictionary<string, string> Params, string requestBody, SyncProduct sync) {
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

            if (response.IsSuccessful) {

                return true;
            } else {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                    Thread.Sleep(5000);

                    bool deleted = ExecDeleteProd(Params, sync.shopifyID);
                    if (deleted == false) {
                        throw new System.Exception($"Couldnt delete {sync.shopifyID}");
                    } else {
                        return false;
                    }

                } else {
                    throw response.ErrorException;
                }
            }
        }

        public bool ExecPutProd(string Endpoint, Dictionary<string, string> Params, string requestBody, SyncProduct sync) {
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

            if (response.IsSuccessful) {

                return true;
            } else {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable) {
                    Thread.Sleep(5000);

                    bool deleted = ExecDeleteProd(Params, sync.shopifyID);
                    if (deleted == false) {
                        throw new System.Exception($"Couldnt delete {sync.shopifyID}");
                    } else {
                        return false;
                    }

                } else {
                    throw response.ErrorException;
                }
            }
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
