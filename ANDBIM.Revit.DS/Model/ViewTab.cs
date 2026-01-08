using Autodesk.Revit.DB.Mechanical;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ANDBIM.Revit.DS
{
    public class ViewTab : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        public ViewTab()
        {
        }
        public ViewTab(string name, ObservableCollection<ViewPanel> panels)
        {
            Name = name;
            Panels = panels;
        }

        public ViewTab(string name)
        {
            Name = name;
        }
        public string Name { get; set; } = "";

        private bool _isChecked { get; set; } = false;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;

                foreach (ViewPanel panel in Panels)
                {
                    panel.IsChecked = value;
                }

                OnPropertyChanged("IsChecked");
            }
        }
        public ObservableCollection<ViewPanel> Panels { get; set; } = new ObservableCollection<ViewPanel>();

    }
}
