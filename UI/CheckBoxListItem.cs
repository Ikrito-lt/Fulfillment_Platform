namespace Ikrito_Fulfillment_Platform.UI {
    public class CheckBoxListItem {

        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public CheckBoxListItem(string Name) {
            this.Name = Name;
            this.IsSelected = true;
        }
    }
}
