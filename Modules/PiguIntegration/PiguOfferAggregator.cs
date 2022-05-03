using Ikrito_Fulfillment_Platform.AWS;
using Ikrito_Fulfillment_Platform.Models;
using Ikrito_Fulfillment_Platform.Modules.PiguIntegration.Models;
using Ikrito_Fulfillment_Platform.Utils;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Ikrito_Fulfillment_Platform.Modules.PiguIntegration
{
    internal static class PiguOfferAggregator
    {
        /// <summary>
        /// gets pigu product offers from database
        /// </summary>
        /// <returns></returns>
        public static List<PiguProductOffer> GetPiguProductOffersDB() { 
            Dictionary<string, PiguProductOffer> productOffers = new Dictionary<string, PiguProductOffer>();

            //getting product offers
            DataBaseInterface db = new();
            var result = db.Table("PiguProductOffers").Get();
            foreach (var pOfferRow in result.Values)
            {
                PiguProductOffer pOffer = new();
                pOffer.SKU = pOfferRow["SKU"];
                pOffer.Title = pOfferRow["TitleLT"];
                pOffer.ProductTypeID = pOfferRow["ProductTypeID"];
                pOffer.ProductTypeVal = pOfferRow["ProductTypeVal"];

                productOffers.Add(pOfferRow["SKU"], pOffer);
            }

            //getting variant offers faster
            result = db.Table("PiguVariantOffers").Get();
            foreach (var vOfferRow in result.Values)
            {
                PiguVariantOffer vOffer = new PiguVariantOffer();
                vOffer.SKU = vOfferRow["SKU"];
                vOffer.Barcode = vOfferRow["Barcode"];
                vOffer.VariantName = vOfferRow["VariantName"];
                vOffer.PiguSupplierCode = vOfferRow["PiguSupplierCode"];
                vOffer.PriceBDiscount = vOfferRow["PriceBeforeDiscount"];
                vOffer.PriceADiscount = vOfferRow["PriceAfterDiscount"];
                vOffer.OurStock = vOfferRow["OurStock"];
                vOffer.OfferCreatedUnixStamp = vOfferRow["DateAdded"];
                vOffer.CollectionHours = vOfferRow["CollectionHours"];

                vOffer.IsEnabled = vOfferRow["IsEnabled"] == "True" ? true : false;

                productOffers[vOffer.SKU].PiguVariantOffers.Add(vOffer);
            }

            return productOffers.Values.ToList();
        }

        /// <summary>
        /// method updates existing pigu product offer in database
        /// </summary>
        /// <param name="pOffer"></param>
        private static void UpdatePiguProductOfferDB(PiguProductOffer pOffer)
        {
            //todo: curently it deletes all rows in database and redoes it
        }

        /// <summary>
        /// method that check if product offer is in database (returns true if exists)
        /// </summary>
        /// <param name="sku"></param>
        private static bool CheckIfOfferExistsInDB(string sku)
        {
            DataBaseInterface db = new();
            var whereGet = new Dictionary<string, Dictionary<string, string>>
            {
                ["SKU"] = new Dictionary<string, string>
                {
                    ["="] = sku
                }
            };
            Dictionary<int, Dictionary<string, string>> result = db.Table("PiguProductOffers").Where(whereGet).Get();
            if (result.Count != 0 && result[0].ContainsValue(sku))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// this method saves offers to database, if offer exits it updates it my calling update method
        /// </summary>
        /// <param name="productOffers"></param>
        private static void SavePiguProductOffersDB(List<PiguProductOffer> productOffers) {
            //todo:change this (deleting all offers from database)
            DataBaseInterface db = new();
            db.ExecuteTextQuery(@"TRUNCATE Table PiguVariantOffers;
                                ALTER TABLE main.PiguVariantOffers DROP FOREIGN KEY PiguVariantOffers_FK;
                                TRUNCATE Table PiguProductOffers;
                                ALTER TABLE main.PiguVariantOffers ADD CONSTRAINT PiguVariantOffers_FK FOREIGN KEY (SKU) REFERENCES main.PiguProductOffers(SKU) ON DELETE CASCADE;");

            //adding to PiguProductOffers Table
            foreach (var pOffer in productOffers)
            {
                //checing if product exists in database if yes Change its status to New
                if (CheckIfOfferExistsInDB(pOffer.SKU))
                {
                    //if exists then update
                    UpdatePiguProductOfferDB(pOffer);
                }
                else {
                    //offer doesnt exist, creating it
                    var InsertData = new Dictionary<string, string>
                    {
                        ["SKU"] = pOffer.SKU,
                        ["TitleLT"] = pOffer.Title,
                        ["ProductTypeID"] = pOffer.ProductTypeID,
                        ["ProductTypeVal"] = pOffer.ProductTypeVal
                    };
                    db.Table("PiguProductOffers").Insert(InsertData);

                    //adding offer variants
                    foreach (var vOffer in pOffer.PiguVariantOffers)
                    {
                        InsertData = new Dictionary<string, string>
                        {
                            ["PiguSupplierCode"] = vOffer.PiguSupplierCode,
                            ["SKU"] = vOffer.SKU,
                            ["Barcode"] = vOffer.Barcode,
                            ["VariantName"] = vOffer.VariantName,
                            ["PriceBeforeDiscount"] = vOffer.PriceBDiscount,
                            ["PriceAfterDiscount"] = vOffer.PriceADiscount,
                            ["OurStock"] = vOffer.OurStock,
                            ["DateAdded"] = vOffer.OfferCreatedUnixStamp,
                            ["CollectionHours"] = vOffer.CollectionHours,
                            ["IsEnabled"] = vOffer.IsEnabled ? "1" : "0"
                        };
                        db.Table("PiguVariantOffers").Insert(InsertData);
                    }
                }
            }
        }

        /// <summary>
        /// Saves PiguProductOffers to daatabasse and then gererates and uploads the xml
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void PostPiguOffers(List<(PiguProductOffer, FullProduct)> productOffers, object sender = null, DoWorkEventArgs e = null)
        {
            //getting all objects ready
            Dictionary<string, FullProduct> reqProducts = new Dictionary<string, FullProduct>();
            Dictionary<string, PiguProductOffer> reqProductOffers = new Dictionary<string, PiguProductOffer>();
            foreach ((var pOffer, var p) in productOffers) {
                string sku = p.SKU;
                reqProducts.Add(sku, p);
                reqProductOffers.Add(sku, pOffer);
            }

            //saving product offers to database
            int promiles = 0;
            BackgroundWorker worker = sender as BackgroundWorker;
            worker.ReportProgress(promiles, (true, $"Saving Pigu Product Sell Offers to the DataBase"));

            SavePiguProductOffersDB(reqProductOffers.Values.ToList());

            //making the list of xPiguProducts
            worker.ReportProgress(promiles, (true, $"Generating XMLs"));
            List<xPiguProduct> xPiguProducts = new List<xPiguProduct>();
            List<xPiguProductStock> xPiguProductStocks = new List<xPiguProductStock>();

            foreach ((var sku, var pOffer) in reqProductOffers) {
                var product = reqProducts[sku];
                
                xPiguProduct xpp = new xPiguProduct();
                xpp.categoryID = pOffer.ProductTypeID;
                xpp.categoryName = pOffer.ProductTypeVal;
                
                xpp.title = product.TitleLT;
                xpp.titleLV = product.TitleLV;
                xpp.titleEE = product.TitleEE;
                xpp.titleRU = product.TitleRU;

                xpp.longDesc = product.DescLT;
                xpp.longDescLV = product.DescLV;
                xpp.longDescEE = product.DescEE;
                xpp.longDescRU = product.DescRU;
                xpp.YTLink = string.Empty;

                foreach (var prop in product.ProductAttributtes) { 
                    xPiguProductProperty xProp = new xPiguProductProperty();
                    xProp.IDstr = prop.Key;
                    xProp.values.Add(prop.Value);
                    xpp.properties.Add(xProp);
                }

                xPiguProductColor xColor = new xPiguProductColor();
                xColor.colorTitle = String.Empty;
                product.Images.ForEach(x => { xColor.images.Add(x); });
                
                foreach (var vOffer in pOffer.PiguVariantOffers)
                {
                    if (!vOffer.IsEnabled) continue;
                    //variant offer in xpp
                    xPiguProductColorModification xcMod = new xPiguProductColorModification();
                    xcMod.modificationTitle = vOffer.VariantName;
                    xcMod.weight = product.Weight.ToString();
                    xcMod.lenght = (product.Lenght / 1000).ToString();
                    xcMod.height = (product.Height / 1000).ToString();
                    xcMod.width = (product.Width / 1000).ToString();
                    xcMod.barcode = vOffer.Barcode;
                    xcMod.supplierCode = vOffer.PiguSupplierCode;
                    xcMod.manufCode = string.Empty;
                    xColor.modifications.Add(xcMod);

                    //variant offer in xpps
                    xPiguProductStock xpps = new xPiguProductStock();
                    xpps.PiguSupplierCode = vOffer.PiguSupplierCode;
                    xpps.Barcode = vOffer.Barcode;
                    xpps.PriceBDiscount = vOffer.PriceBDiscount;
                    xpps.PriceADiscount = vOffer.PriceADiscount;
                    xpps.OurStock = vOffer.OurStock;
                    xpps.CollectionHours = vOffer.CollectionHours;
                    xPiguProductStocks.Add(xpps);
                }
                xpp.colors.Add(xColor);
                xPiguProducts.Add(xpp);
            }

            //making product xml string
            StringBuilder psb = new StringBuilder();
            psb.Append("<products>");
            xPiguProducts.ForEach(xpp => psb.Append(xpp.GetXml()));
            psb.Append("</products>");

            //making product stock xml string
            StringBuilder pssb = new StringBuilder();
            pssb.Append("<products>");
            xPiguProductStocks.ForEach(xpps => pssb.Append(xpps.GetXml()));
            pssb.Append("</products>");

            //creating Porduct xml
            XmlDocument xPiguProductsXml = new XmlDocument();
            xPiguProductsXml.LoadXml(psb.ToString());

            XmlDeclaration xmlProductDec;
            xmlProductDec = xPiguProductsXml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement productRoot =xPiguProductsXml.DocumentElement;
            xPiguProductsXml.InsertBefore(xmlProductDec, productRoot);

            //creating product stock xml
            XmlDocument xPiguProductStocksXml = new XmlDocument();
            xPiguProductStocksXml.LoadXml(pssb.ToString());

            XmlDeclaration xmlProductStockDec;
            xmlProductStockDec = xPiguProductStocksXml.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement productStockRoot = xPiguProductStocksXml.DocumentElement;
            xPiguProductStocksXml.InsertBefore(xmlProductStockDec, productStockRoot);

            //todo: xml Validation
            //uploading XMls to AWS
            worker.ReportProgress(promiles, (true, $"Uploading XMLs to AWS Buckets"));

            //uploading product xml and passing results

            Dictionary<string, string> uploadRes = new();
            var productUploadTask = S3UploadXmlAsync("pigu-product-xml", "piguProductXml.xml", xPiguProductsXml);
            uploadRes.Add("productsXml", productUploadTask.GetAwaiter().GetResult());
            var productStockUploadTask = S3UploadXmlAsync("pigu-stock-xml", "piguStockXml.xml", xPiguProductStocksXml);
            uploadRes.Add("productStocksXml", productStockUploadTask.GetAwaiter().GetResult());
            e.Result = uploadRes;
        }

        /// <summary>
        ///  method that is used to upload xml file to 
        /// </summary>
        /// <param name="bucketName"></param>
        /// <param name="keyName"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        private static Task<string> S3UploadXmlAsync( string bucketName, string keyName, XmlDocument xml )
        {
            return Task.Run<string>(async () =>
            {
                string path = Directory.GetCurrentDirectory();
                var filePath = path + $"\\{keyName}";
                xml.Save(filePath);

                var aWS3UploaderMessage = await AWS3Uploader.UploadFileAsync(bucketName, keyName, filePath);
                return aWS3UploaderMessage;
            });
        }
    }
}
