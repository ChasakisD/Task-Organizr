using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace TDLApi.DataModel
{
    public class HamburgerItem : INotifyPropertyChanged
    {
        public Symbol Icon { get; set; }
        public string Name { get; set; }
        public Type PageType { get; set; }
        public string Image { get; set; }
        public string Email { get; set; }
        public string Line { get; set; }
        public bool containsPanel { get; set; }
        private string rect { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string updateScore)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(updateScore));
        }
        public string hasRect
        {
            get
            {
                return rect;
            }
            set
            {
                rect = value;
                NotifyPropertyChanged("hasRect");  // Trigger the change event if the value is changed!
            }
        }
    }
}
