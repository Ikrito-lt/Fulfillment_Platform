using CsvHelper;
using CsvHelper.Configuration;
using Ikrito_Fulfillment_Platform.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules {
    class CSVModule {

        private readonly List<dynamic> csvArchive;
        public CSVModule() {
            csvArchive = GetCSV();
        }

        private List<dynamic> GetCSV() {

            List<dynamic> records = new();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
                PrepareHeaderForMatch = args => args.Header.Replace(" ", "_")
            };

            using (var reader = new StreamReader("C:\\Users\\Luke\\Desktop\\Ikrito_Fulfillment_Platform\\Files\\CSVarchive\\products_export_1.csv"))
            using (var csv = new CsvReader(reader, config)) {
                records.AddRange(csv.GetRecords<dynamic>().ToList());
            }

            using (var reader = new StreamReader("C:\\Users\\Luke\\Desktop\\Ikrito_Fulfillment_Platform\\Files\\CSVarchive\\products_export_2.csv"))
            using (var csv = new CsvReader(reader, config)) {
                records.AddRange(csv.GetRecords<dynamic>().ToList());
            }

            using (var reader = new StreamReader("C:\\Users\\Luke\\Desktop\\Ikrito_Fulfillment_Platform\\Files\\CSVarchive\\products_export_2.csv"))
            using (var csv = new CsvReader(reader, config)) {
                records.AddRange(csv.GetRecords<dynamic>().ToList());
            }

            using (var reader = new StreamReader("C:\\Users\\Luke\\Desktop\\Ikrito_Fulfillment_Platform\\Files\\CSVarchive\\products_export_3.csv"))
            using (var csv = new CsvReader(reader, config)) {
                records.AddRange(csv.GetRecords<dynamic>().ToList());
            }

            //removing ones without sku
            records.RemoveAll(r => r.Variant_SKU.Length == 0);
            return records;
        }

        public void ImportTagsToDB() {
            List<Product> products = ProductModule.GetAllProducts();

            foreach (var p in products) {
                var sku = p.sku;

                for (int i = 0; i < csvArchive.Count; i++) {
                    var c = csvArchive[i];
                    string csku = (c.Variant_SKU as string).Trim();
                    string cTag = (c.Tags as string).Trim();
                    
                    if (csku.Contains(sku) || sku.Contains(csku)) {
                        Debug.WriteLine($"{sku}: >> {cTag}");
                        List<string> cTags = cTag.Split(",").ToList();
                        cTags.ForEach(x => x.Trim());
                        p.tags.AddRange(cTags);
                        saveProdToDB(p);

                    } else {
                        continue;
                    }
                }
            }
        }

        void saveProdToDB(Product p) {
            DataBaseInterface db = new();
            foreach (string tag in p.tags) {
                var insertData = new Dictionary<string, string> {
                    ["ProductID"] = p.DBID.ToString(),
                    ["Tag"] = tag.Trim()
                };

                db.Table("TDB_Tags").Insert(insertData);

                ProductModule.MarkProductForShopSync(p.sku);
            }

        }

    }
}
