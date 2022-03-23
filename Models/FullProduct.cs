using System;
using System.Collections.Generic;
using System.Linq;

namespace Ikrito_Fulfillment_Platform.Models
{
    /// <summary>
    /// class for storring full product module
    /// </summary>
    public class FullProduct
    {
        public string TitleLT { set; get; }
        public string TitleLV { set; get; }
        public string TitleEE { set; get; }
        public string TitleRU { set; get; }
        public string DescLT { set; get; }
        public string DescLV { set; get; }
        public string DescEE { set; get; }
        public string DescRU { set; get; }
        public string Vendor { set; get; }
        public string ProductTypeID { set; get; }        //category ID
        public string SKU { set; get; }
        public double Weight { set; get; }              //in kg
        public int Height { set; get; }                 //in mm
        public int Lenght { set; get; }                 //in mm
        public int Width { set; get; }                  //in mm
        public string ProductTypeVendor { set; get; }   //for saving vendor product type to database.
        public string DeliveryTime { set; get; }        //delivery time string
        public string AddedTimeStamp { set; get; }      //timestamp of when product was created
        public DateTime GetAddedTime()
        {

            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            double timestamp = double.Parse(AddedTimeStamp);
            dateTime = dateTime.AddSeconds(timestamp).ToLocalTime();
            return dateTime;
        }

        public List<string> Tags = new();
        public List<string> Images = new();
        public List<ProductVariant> ProductVariants = new();

        public Dictionary<string, string> ProductAttributtes = new();

        //for util
        public string Status { set; get; }              //for product status as in sync Status
        public string ProductTypeDisplayVal { set; get; }   // for saving category text val

        //for showing 
        public string VariantCount => ProductVariants?.Count > 0 ? ProductVariants.Count.ToString() : "0";
        public string FirstVariantOurStock => ProductVariants?.Count > 0 ? ProductVariants.First().OurStock.ToString() : "NaN";
        public string FirstVariantVendorStock => ProductVariants?.Count > 0 ? ProductVariants.First().VendorStock.ToString() : "NaN";
        public string FirstVariantPrice => ProductVariants?.Count > 0 ? ProductVariants.First().Price.ToString() : "NaN";
        public string FirstVariantVendorPrice => ProductVariants?.Count > 0 ? ProductVariants.First().PriceVendor.ToString() : "NaN";

        /// <summary>
        /// class for describing product variants
        /// </summary>
        public class ProductVariant
        {
            public int VariantDBID { set; get; }
            public double Price { set; get; }
            public int VendorStock { set; get; }
            public int OurStock { set; get; }
            public string Barcode { set; get; }
            public double PriceVendor { set; get; }
            public string VariantType { set; get; }
            public string VariantData { set; get; }
            public bool PermPrice { set; get; }

            public bool isSame(ProductVariant other)
            {
                if (Barcode != other.Barcode) return false;
                if (PriceVendor != other.PriceVendor) return false;
                if (VendorStock != other.VendorStock) return false;
                return true;
            }
        }

        public FullProduct()
        {
            //to detect unassigned fields
            TitleLT = "NULL";
            DescLT = "NULL";
            Vendor = "NULL";
            ProductTypeID = "NULL";
            SKU = "NULL";
            Weight = 0;
            Height = 1;
            Lenght = 1;
            Width = 1;
            DeliveryTime = "-";
        }

        //TODO: think about adding SEO shit
    }
}
