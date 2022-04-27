using System.Collections.Generic;

namespace Ikrito_Fulfillment_Platform.Models
{
    internal class CategoryTree
    {
        public List<TopLevelCategory> CategoryTreeRoot;
        
        //top level categories (like: "Vaikams ir kūdikiams")
        internal class TopLevelCategory {
            public string CatName;
            public List<SubCategory> children; 

            //sub categories (like: "Drabužiai, avalynė vaikams ir kūdikiams")  
            internal class SubCategory {
                public string CatName;
                public List<LeafCategory> children;

                //leaf categories (like: "Drabužiai kūdikiams")
                internal class LeafCategory {
                    public int CatID;
                    public string CatName;
                }
            }
        }
    }
}
