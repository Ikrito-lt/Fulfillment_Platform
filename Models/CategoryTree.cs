using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ikrito_Fulfillment_Platform.Models
{
    internal class CategoryTree
    {
        public CategoryTree()
        {
            children = new ObservableCollection<CategoryTree>();
        }
        public string CatName { get; set; }
        public int? CatID { get; set; }
        public ObservableCollection<CategoryTree> children { get; set; }
    }
}
