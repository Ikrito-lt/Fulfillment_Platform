using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ikrito_Fulfillment_Platform.UI {
    public class CheckBoxListItem{

        public string Name {
            get { return _name; }
            set {
                
                _name = value; }
        }
        private string _name;

        public bool IsSelected {
            get { return _isSelected; }
            set { _isSelected = value; }
        }
        private bool _isSelected;

        public CheckBoxListItem(string Name) {
            this.Name = Name;
            this.IsSelected = true;
        }

    }
}
