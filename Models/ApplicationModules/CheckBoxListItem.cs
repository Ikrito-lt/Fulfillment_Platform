namespace Ikrito_Fulfillment_Platform.UI {
    public class CheckBoxListItem {
        //is used to filter products by status in productPage datagrid
        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public CheckBoxListItem(string Name) {
            this.Name = Name;
            this.IsSelected = true;
        }
    }
}
