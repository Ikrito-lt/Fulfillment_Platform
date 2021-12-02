using Ikrito_Fulfillment_Platform.Modules.Supplier.KotrynaGroup.Modules;
using Ikrito_Fulfillment_Platform.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier {
    class KGModule {
        public const string KGApi = Globals.KGApi;
        public const string KGApiKey = Globals.KGApiKey;

        private const string _BaseUrl = KGApi + "data/";
        private static readonly Dictionary<string, string> _APIHeader = new Dictionary<string, string>()
        {
            { "API-KEY", KGApiKey },
        };

        private const string _CataloguePath = "assortment";                     //get
        private const string _InfoPath = "products_information";                //post
        private const string _MeasurementsPath = "products_measurements";       //post
        private const string _PackagingPath = "products_packaging";             //post
        private const string _HierarchyPath = "hierarchy";                      //post

        private static readonly Lazy<List<AssortmentProduct>> _LazyAssortmentList = new(() => GetAssortmentList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<AssortmentProduct> _AssortmentList => _LazyAssortmentList.Value;

        private static readonly Lazy<List<ProductInfo>> _LazyProductInfoList = new(() => GetProductInfoList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<ProductInfo> _ProductInfoList => _LazyProductInfoList.Value;

        private static readonly Lazy<List<ProductMeasurements>> _LazyProductMeasurementsList = new(() => GetProductMeasurementsList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<ProductMeasurements> _ProductMeasurementsList => _LazyProductMeasurementsList.Value;

        private static readonly Lazy<List<ProductPackaging>> _LazyProductPackagingList = new(() => GetProductPackagingList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<ProductPackaging> _ProductPackagingList => _LazyProductPackagingList.Value;


        //
        // section of methods for getting data from KG API
        //

        //downloads product catalogue from KG API
        private static List<AssortmentProduct> GetAssortmentList() {
            RESTClient restClient = new(_BaseUrl);
            string assortmentJson = restClient.ExecGetParams(_CataloguePath, _APIHeader);
            dynamic assortmentResponse = JsonConvert.DeserializeObject<dynamic>(assortmentJson);

            List<AssortmentProduct> assortmentProducts = new();

            if (assortmentResponse["status"] == true) {
                dynamic dynamicCat = assortmentResponse["result"];
                foreach (var prod in dynamicCat) {
                    if (string.IsNullOrEmpty(prod["base_price"].ToString()) || string.IsNullOrEmpty(prod["price"].ToString())) {
                        continue;
                    } else{
                        AssortmentProduct product = new();
                        product.id = prod["id"];
                        product.axapta_id = prod["axapta_id"];
                        product.ean = prod["ean"];
                        product.qty = prod["qty"];
                        product.base_price = double.Parse(prod["base_price"].ToString());
                        product.price = double.Parse(prod["price"].ToString());
                        product.discount = prod["discount"];
                        product.newProduct = prod["new"];

                        assortmentProducts.Add(product);
                    }
                }
            }
            return assortmentProducts;
        }

        //downloads product info from KG API
        private static List<ProductInfo> GetProductInfoList() {
            List<ProductInfo> infoList = new();

            for (int i = 0; i < _AssortmentList.Count; i = i + 200) {
                var items = _AssortmentList.Skip(i).Take(200);
                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();

                foreach (AssortmentProduct item in items) {
                    postData.Add(new KeyValuePair<string, string>("ids[]", item.axapta_id));
                }

                HttpClient client = new();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{_BaseUrl}{_InfoPath}");
                request.Headers.Add("API-KEY", KGApiKey);
                request.Content = new FormUrlEncodedContent(postData);
                var response = client.Send(request);

                Stream receiveStream = response.Content.ReadAsStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string infoJson = readStream.ReadToEnd();
                dynamic infoResponse = JsonConvert.DeserializeObject<dynamic>(infoJson);

                if (infoResponse["status"] == true) {
                    dynamic dynamicInfo = infoResponse["result"];
                    foreach (var info in dynamicInfo) {
                        ProductInfo pInfo = new();

                        pInfo.axapta_id = info.Name;
                        pInfo.kpn = info.First["additional"]["kpn"];
                        pInfo.coo = info.First["additional"]["coo"];
                        pInfo.title = info.First["titles"]["lt"];

                        //getting images
                        if (info.First["images"] != null) {
                            foreach (string image in info.First["images"]) {
                                pInfo.images.Add(image);
                            }
                        }

                        //getting vendor type
                        string type = (string)info.First["hierarchy"]["business_group"]["titles"]["lt"] + " / " +
                                      (string)info.First["hierarchy"]["division_group"]["titles"]["lt"] + " / " +
                                      (string)info.First["hierarchy"]["department_group"]["titles"]["lt"] + " / " +
                                      (string)info.First["hierarchy"]["retail_group"]["titles"]["lt"];
                        pInfo.vendorType = type;

                        //getting properties
                        foreach (var prop in info.First["properties"]) {
                            string key = (string)prop.First["title"];
                            string val = prop.First["values"].ToString();
                            val = val.Split(":")[1];
                            val = val.Split("\"")[1];

                            if (key == "Other") {
                                key = val;
                            }
                            pInfo.properties.Add(key, val);
                        }

                        //getting brand
                        if (pInfo.properties.ContainsKey("Brand")) {
                            pInfo.brand = pInfo.properties["Brand"];
                        }

                        infoList.Add(pInfo);
                    }
                } else {
                    string error = infoResponse["error"];
                    string method = _InfoPath;
                    //todo: make handlig for that
                    throw new Exception($"{method}: {error}");
                }
            }
            return infoList;
        }

        //downloads product measurements from KG API
        private static List<ProductMeasurements> GetProductMeasurementsList() {
            List<ProductMeasurements> MeasurementsList = new();

            for (int i = 0; i < _AssortmentList.Count; i = i + 200) {
                var items = _AssortmentList.Skip(i).Take(200);
                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();

                foreach (AssortmentProduct item in items) {
                    postData.Add(new KeyValuePair<string, string>("ids[]", item.axapta_id));
                }

                HttpClient client = new();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{_BaseUrl}{_MeasurementsPath}");
                request.Headers.Add("API-KEY", KGApiKey);
                request.Content = new FormUrlEncodedContent(postData);
                var response = client.Send(request);

                Stream receiveStream = response.Content.ReadAsStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string MeasurementsJson = readStream.ReadToEnd();
                dynamic MeasurementsResponse = JsonConvert.DeserializeObject<dynamic>(MeasurementsJson);

                if (MeasurementsResponse["status"] == true) {
                    dynamic dynamicMeasurements = MeasurementsResponse["result"];
                    foreach (var m in dynamicMeasurements) {
                        var json = JsonConvert.SerializeObject(m.First);
                        ProductMeasurements PM = JsonConvert.DeserializeObject<ProductMeasurements>(json);
                        PM.axapta_id = m.Name;

                        MeasurementsList.Add(PM);

                    }
                } else {
                    string error = MeasurementsResponse["error"];
                    string method = _MeasurementsPath;
                    //todo: make handlig for that
                    throw new Exception($"{method}: {error}");
                }
            }
            return MeasurementsList;
        }

        //downloads product measurements from KG API
        private static List<ProductPackaging> GetProductPackagingList() {
            List<ProductPackaging> PackagingList = new();

            for (int i = 0; i < _AssortmentList.Count; i = i + 200) {
                var items = _AssortmentList.Skip(i).Take(200);
                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();

                foreach (AssortmentProduct item in items) {
                    postData.Add(new KeyValuePair<string, string>("ids[]", item.axapta_id));
                }

                HttpClient client = new();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{_BaseUrl}{_PackagingPath}");
                request.Headers.Add("API-KEY", KGApiKey);
                request.Content = new FormUrlEncodedContent(postData);
                var response = client.Send(request);

                Stream receiveStream = response.Content.ReadAsStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string PackagingJson = readStream.ReadToEnd();
                dynamic PackagingResponse = JsonConvert.DeserializeObject<dynamic>(PackagingJson);

                if (PackagingResponse["status"] == true) {
                    dynamic dynamicPackaging = PackagingResponse["result"];
                    foreach (var p in dynamicPackaging) {

                        ProductPackaging PP = new();
                        PP.axapta_id = p.Name;

                        foreach (var pack in p.First) {
                            var json = JsonConvert.SerializeObject(pack);
                            Packaging P = JsonConvert.DeserializeObject<Packaging>(json);
                            PP.packagings.Add(P);

                        }
                        PackagingList.Add(PP);
                    }
                } else {
                    string error = PackagingResponse["error"];
                    string method = _PackagingPath;
                    //todo: make handlig for that
                    throw new Exception($"{method}: {error}");
                }
            }
            return PackagingList;
        }




    }
}
