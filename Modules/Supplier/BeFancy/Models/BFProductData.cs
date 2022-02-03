using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.Modules.Supplier.BeFancy.Models
{
    internal class BFProduct
    {
        public string id { get; set; }//
        public string model { get; set; }// is empty
        public string category { get; set; }//
        public string title { get; set; }//
        public string description { get; set; }//
        public double price { get; set; }//
        public double oldPrice { get; set; }
        public string manufacturer { get; set; }//
        public string deliveryTimeText { get; set; }//
        public List<string> imageURLs { get; set; }//
        public Dictionary<string, string> attributes { get; set; }//
        public string group { get; set; }//same as id
        public int stock { set; get; }//
        public string barcode { set; get; }//
    }

    internal class BFProductVariant
    {
        public string variantTitle { get; set; }
        public string variantDescription { get; set; }
        public int stock { get; set; }
        public string barcode { set; get; }
    }

    internal class BFProductWithVariants
    {
        public string id { get; set; }//
        public string model { get; set; }//is empty
        public string category { get; set; }//
        public string title { get; set; }//
        public string description { get; set; }//
        public double price { get; set; }//
        public double oldPrice { get; set; }//
        public string manufacturer { get; set; }//
        public string deliveryTimeText { get; set; }//
        public List<string> imageURLs { get; set; }//
        public Dictionary<string, string> attributes { get; set; }//
        public string group { get; set; }//same as id
        public List<BFProductVariant> variants { get; set; }//
    }
}
