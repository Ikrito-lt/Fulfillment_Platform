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

        private const string _SKUPrefix = "KG-";

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
                    } else {
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


        //
        // Section for getting Product form KG API
        //

        // buulding List<Product> from KG api data
        public static List<Product> BuildProductList() {
            List<Product> pList = new();

            foreach (AssortmentProduct AP in _AssortmentList) {
                string ProductID = AP.axapta_id;
                ProductInfo PI = _ProductInfoList.Find(x => x.axapta_id == ProductID);
                ProductMeasurements PM = _ProductMeasurementsList.Find(x => x.axapta_id == ProductID);
                ProductPackaging PP = _ProductPackagingList.Find(x => x.axapta_id == ProductID);

                pList.Add(BuildProduct(AP, PI, PM, PP));
            }

            return pList;
        }

        // method that builds Product form API data
        public static Product BuildProduct(AssortmentProduct AP, ProductInfo PI, ProductMeasurements PM, ProductPackaging PP) {
            Product newProduct = new();

            newProduct.title = SQLUtil.SQLSafeString(PI.title);
            newProduct.body_html = BuildDescription(PI.properties);

            string newVendor = PI.brand;
            if (string.IsNullOrEmpty(newVendor)) {
                newVendor = "NULL_ERROR";
            }

            newProduct.vendor = SQLUtil.SQLSafeString(newVendor);
            newProduct.product_type = "Not-Assigned";
            newProduct.price = AP.base_price;
            newProduct.sku = _SKUPrefix + AP.axapta_id;
            newProduct.stock = AP.qty;
            newProduct.barcode = AP.ean;
            newProduct.vendor_price = AP.price;

            newProduct.productTypeVendor = PI.vendorType;

            newProduct.images = PI.images;
            //no tags in new products;

            //getting the dimensions
            newProduct.weight = double.Parse(PM.net_weight);
            newProduct.height = (int)Math.Round(double.Parse(PM.gross_height) * 1000);
            newProduct.lenght = (int)Math.Round(double.Parse(PM.gross_depth) * 1000);
            newProduct.width = (int)Math.Round(double.Parse(PM.gross_width) * 1000);

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


        //
        // Section for automatically updating and adding products to database
        //

        // method for updates KG Products
        public void UpdateKGProducts(object sender = null, DoWorkEventArgs e = null) {
            List<Product> DBProducts = ProductModule.GetKGProducts();
            List<Product> APIProducts = BuildProductList();

            List<Product> ArchiveProducts = DBProducts.Where(p1 => APIProducts.All(p2 => p2.sku != p1.sku)).ToList();
            List<Product> NewProducts = APIProducts.Where(p1 => DBProducts.All(p2 => p2.sku != p1.sku)).ToList();
            List<Product> UpdateProducts = APIProducts.Where(p1 => NewProducts.All(p2 => p2.sku != p1.sku)).ToList();

            //remove dublicate skus from newProd list
            var a = NewProducts.GroupBy(x => x.sku.ToLower()).Where(x => x.LongCount() > 1).ToList();
            a.ForEach(x => NewProducts.RemoveAll(y => y.sku.ToLower() == x.Key));

            Dictionary<string, Dictionary<string, string>> appliedChanges = new();          //for updates
            List<Dictionary<string, string>> newChanges = new();                            //for new products
            List<Dictionary<string, string>> archivedChanges = new();                       //for archived Products                     

            //archiving products
            foreach (Product archiveProduct in ArchiveProducts) {
                try {
                    ProductModule.ChangeProductStatus(archiveProduct.sku, ProductStatus.NeedsArchiving);

                    Dictionary<string, string> archiveChange = new();
                    archiveChange.Add("SKU", archiveProduct.sku);
                    archiveChange.Add("PriceVendor", archiveProduct.vendor_price.ToString());
                    archiveChange.Add("Stock", archiveProduct.stock.ToString());
                    archiveChange.Add("Barcode", archiveProduct.barcode);
                    archiveChange.Add("Vendor", archiveProduct.vendor);
                    archiveChange.Add("VendorType", archiveProduct.productTypeVendor);
                    archivedChanges.Add(archiveChange);
                }
                catch (Exception ex) {
                    MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            // adding new Products
            foreach (Product newProduct in NewProducts) {
                ProductModule.AddProductToDB(newProduct);

                Dictionary<string, string> newChange = new();
                newChange.Add("SKU", newProduct.sku);
                newChange.Add("PriceVendor", newProduct.vendor_price.ToString());
                newChange.Add("Stock", newProduct.stock.ToString());
                newChange.Add("Barcode", newProduct.barcode);
                newChange.Add("Vendor", newProduct.vendor);
                newChange.Add("VendorType", newProduct.productTypeVendor);
                newChanges.Add(newChange);
            }

            DataBaseInterface db = new();

            //updating products
            foreach (Product updateProduct in UpdateProducts) {
                Product oldProduct = DBProducts.Find(x => x.sku == updateProduct.sku);

                //if no changes skip
                if (updateProduct.stock == oldProduct.stock && updateProduct.vendor_price == oldProduct.vendor_price) {
                    continue;
                } else {

                    appliedChanges.Add(oldProduct.sku, new Dictionary<string, string>() {
                        ["Stock"] = "",
                        ["PriceVendor"] = "",
                        ["Price"] = "",
                    });

                    //update stock
                    if (updateProduct.stock != oldProduct.stock) {
                        var stockUpdateData = new Dictionary<string, string> {
                            ["Stock"] = updateProduct.stock.ToString()
                        };
                        var stockWhereUpdate = new Dictionary<string, Dictionary<string, string>> {
                            ["SKU"] = new Dictionary<string, string> {
                                ["="] = oldProduct.sku
                            }
                        };
                        db.Table("KG_Products").Where(stockWhereUpdate).Update(stockUpdateData);

                        //adding change to applied change list
                        appliedChanges[oldProduct.sku]["Stock"] = $"{oldProduct.stock} -> {updateProduct.stock}";
                    }

                    //update price
                    if (updateProduct.vendor_price != oldProduct.vendor_price) {
                        //updating price value
                        var priceUpdateData = new Dictionary<string, string> {
                            ["PriceVendor"] = updateProduct.vendor_price.ToString(),
                            ["Price"] = updateProduct.price.ToString()
                        };
                        var priceWhereUpdate = new Dictionary<string, Dictionary<string, string>> {
                            ["SKU"] = new Dictionary<string, string> {
                                ["="] = oldProduct.sku
                            }
                        };
                        db.Table("KG_Products").Where(priceWhereUpdate).Update(priceUpdateData);

                        //adding change to applied change list
                        appliedChanges[oldProduct.sku]["PriceVendor"] = $"{oldProduct.vendor_price} -> {updateProduct.vendor_price}";
                        appliedChanges[oldProduct.sku]["Price"] = $"{oldProduct.price} -> {updateProduct.price}";
                    }

                    //updating product status
                    try {
                        ProductModule.ChangeProductStatus(oldProduct.sku, ProductStatus.WaitingShopSync);
                    }
                    catch (Exception ex) {
                        MessageBox.Show("An exception just occurred:\n" + ex.Message + "\n\nSend screenshot you know where.", "Change Product Status Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            //pass applied changes and pending changes to update on complete method
            Dictionary<string, object> changes = new();
            changes.Add("UpdatedProducts", appliedChanges);
            changes.Add("ArchivedProducts", archivedChanges);
            changes.Add("NewProducts", newChanges);
            if (e != null) {
                e.Result = changes;
            }
        }
    }
}
