using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules.Supplier.KotrynaGroup.Modules;
using Ikrito_Fulfillment_Platform.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Ikrito_Fulfillment_Platform.Models.FullProduct;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier.KotrynaGroup {
    static class KGModule {
        private const string KGApi = Globals.KGApi;
        private const string KGApiKey = Globals.KGApiKey;

        private static readonly Dictionary<string, string> _APIHeader = new Dictionary<string, string>()
        {
            { "API-KEY", KGApiKey },
        };

        private const string _BaseUrl = KGApi + "data/";
        private const string _CataloguePath = "assortment";                     //get
        private const string _InfoPath = "products_information";                //post
        private const string _MeasurementsPath = "products_measurements";       //post
        private const string _PackagingPath = "products_packaging";             //post
        private const string _HierarchyPath = "hierarchy";                      //post

        private const string _SKUPrefix = "KG-";

        private static readonly Lazy<List<KGAssortmentProduct>> _LazyAssortmentList = new(() => GetAssortmentList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static List<KGAssortmentProduct> _AssortmentList => _LazyAssortmentList.Value;

        private static readonly Lazy<List<KGProductInfo>> _LazyProductInfoList = new(() => GetProductInfoList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static List<KGProductInfo> _ProductInfoList => _LazyProductInfoList.Value;

        private static readonly Lazy<List<KGProductMeasurements>> _LazyProductMeasurementsList = new(() => GetProductMeasurementsList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static List<KGProductMeasurements> _ProductMeasurementsList => _LazyProductMeasurementsList.Value;

        private static readonly Lazy<List<KGProductPackaging>> _LazyProductPackagingList = new(() => GetProductPackagingList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private static List<KGProductPackaging> _ProductPackagingList => _LazyProductPackagingList.Value;

        private static readonly Lazy<List<FullProduct>> _LazyProductList = new(() => BuildProductList());
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        public static List<FullProduct> ProductList => _LazyProductList.Value;


        //
        // section of methods for getting data from KG API
        //

        //downloads product catalogue from KG API
        private static List<KGAssortmentProduct> GetAssortmentList() {
            RESTClient restClient = new(_BaseUrl);
            string assortmentJson = restClient.ExecGetParams(_CataloguePath, _APIHeader);
            dynamic assortmentResponse = JsonConvert.DeserializeObject<dynamic>(assortmentJson);

            List<KGAssortmentProduct> assortmentProducts = new();

            if (assortmentResponse["status"] == true) {
                dynamic dynamicCat = assortmentResponse["result"];
                foreach (var prod in dynamicCat) {
                    if (string.IsNullOrEmpty(prod["base_price"].ToString()) || string.IsNullOrEmpty(prod["price"].ToString())) {
                        continue;
                    } else {
                        KGAssortmentProduct product = new();
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
        private static List<KGProductInfo> GetProductInfoList() {
            List<KGProductInfo> infoList = new();

            for (int i = 0; i < _AssortmentList.Count; i = i + 200) {
                var items = _AssortmentList.Skip(i).Take(200);
                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();

                foreach (KGAssortmentProduct item in items) {
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
                        KGProductInfo pInfo = new();

                        pInfo.axapta_id = info.Name;
                        pInfo.kpn = info.First["additional"]["kpn"];
                        pInfo.coo = info.First["additional"]["coo"];
                        pInfo.title = info.First["titles"]["lt"];

                        if (pInfo.title == "null" || string.IsNullOrEmpty(pInfo.title)) {
                            continue;
                        }

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
                    throw new Exception($"{method}: {error}");
                }
            }
            return infoList;
        }

        //downloads product measurements from KG API
        private static List<KGProductMeasurements> GetProductMeasurementsList() {
            List<KGProductMeasurements> MeasurementsList = new();

            for (int i = 0; i < _AssortmentList.Count; i = i + 200) {
                var items = _AssortmentList.Skip(i).Take(200);
                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();

                foreach (KGAssortmentProduct item in items) {
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
                        KGProductMeasurements PM = JsonConvert.DeserializeObject<KGProductMeasurements>(json);
                        PM.axapta_id = m.Name;

                        MeasurementsList.Add(PM);

                    }
                } else {
                    string error = MeasurementsResponse["error"];
                    string method = _MeasurementsPath;
                    throw new Exception($"{method}: {error}");
                }
            }
            return MeasurementsList;
        }

        //downloads product measurements from KG API
        private static List<KGProductPackaging> GetProductPackagingList() {
            List<KGProductPackaging> PackagingList = new();

            for (int i = 0; i < _AssortmentList.Count; i = i + 200) {
                var items = _AssortmentList.Skip(i).Take(200);
                List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();

                foreach (KGAssortmentProduct item in items) {
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

                        KGProductPackaging PP = new();
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
                    throw new Exception($"{method}: {error}");
                }
            }
            return PackagingList;
        }


        //
        // Section for getting Product form KG API
        //

        // bulding List<Product> from KG api data
        private static List<FullProduct> BuildProductList() {
            List<FullProduct> pList = new();

            foreach (KGAssortmentProduct AP in _AssortmentList) {
                string ProductID = AP.axapta_id;
                KGProductInfo PI = _ProductInfoList.Find(x => x.axapta_id == ProductID);
                KGProductMeasurements PM = _ProductMeasurementsList.Find(x => x.axapta_id == ProductID);
                KGProductPackaging PP = _ProductPackagingList.Find(x => x.axapta_id == ProductID);

                pList.Add(BuildProduct(AP, PI, PM, PP));
            }

            return pList;
        }

        // method that builds Product form API data
        private static FullProduct BuildProduct(KGAssortmentProduct AP, KGProductInfo PI, KGProductMeasurements PM, KGProductPackaging PP) {
            FullProduct newProduct = new();

            newProduct.Title = SQLUtil.SQLSafeString(PI.title);
            newProduct.HTMLBody = BuildDescription(PI.properties);

            string newVendor = PI.brand;
            if (string.IsNullOrEmpty(newVendor))
            {
                newVendor = "NULL_ERROR";
            }

            newProduct.Vendor = SQLUtil.SQLSafeString(newVendor);
            newProduct.ProductTypeID = 1.ToString();
            newProduct.SKU = _SKUPrefix + AP.axapta_id;

            newProduct.ProductTypeVendor = PI.vendorType;

            newProduct.Images = PI.images;
            //no tags in new products;

            //getting the dimensions
            newProduct.Weight = double.Parse(PM.net_weight);
            newProduct.Height = (int)Math.Round(double.Parse(PM.gross_height) * 1000);
            newProduct.Lenght = (int)Math.Round(double.Parse(PM.gross_depth) * 1000);
            newProduct.Width = (int)Math.Round(double.Parse(PM.gross_width) * 1000);

            if (newProduct.Height == 0) newProduct.Height = 1;
            if (newProduct.Width == 0) newProduct.Width = 1;
            if (newProduct.Lenght == 0) newProduct.Lenght = 1;

            //adding product added timestamp
            newProduct.AddedTimeStamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds().ToString();

            //adding new Variant (only one cause no variants in KG)
            ProductVariant newVariant = new();
            newVariant.Stock = AP.qty;
            newVariant.Barcode = AP.ean;
            newVariant.PriceVendor = AP.price;
            newVariant.Price = AP.base_price;
            newProduct.ProductVariants.Add(newVariant);

            //if properties arent empty add them
            if (PI.properties.Count > 0) {
                newProduct.ProductAttributtes = PI.properties;
            }

            return newProduct;
        }

        //method that builds description for the product uisng datasheet KVP
        private static string BuildDescription(Dictionary<string, string> prodDataKVP) {
            string description = "";
            Dictionary<string, string> prodDataKVPNew = new();

            foreach (var pair in prodDataKVP) {
                if (pair.Key == pair.Value) {
                    description += pair.Key + "<br><br>";
                } else {
                    prodDataKVPNew.Add(pair.Key, pair.Value);
                }
            }

            prodDataKVP = prodDataKVPNew;

            StringBuilder sb = new();
            using (HTMLTable table = new(sb)) {
                foreach (var kvp in prodDataKVP) {
                    using (HTMLRow row = table.AddRow()) {
                        row.AddCell(kvp.Key);
                        row.AddCell(kvp.Value);
                    }
                }
            }

            string finishedTable = sb.ToString();
            description += finishedTable;

            description = description.Replace("\'", $"\\'");
            description = description.Replace("\"", $"\\\"");

            return description;
        }
    }
}
