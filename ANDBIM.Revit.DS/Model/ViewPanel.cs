using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ANDBIM.Revit.DS
{
    public class ViewPanel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        public ViewPanel(string name, ObservableCollection<ViewButtonGroup> buttonGroups)
        {
            Name = name;
            ButtonGroups = buttonGroups;
        }
        public ViewPanel(string name)
        {
            Name = name;
        }
        public ViewPanel()
        {
        }
        public string Name { get; set; } = "";
        private bool _isChecked { get; set; } = false;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;

                foreach (ViewButtonGroup group in ButtonGroups)
                {
                    foreach (ViewButton button in group.Buttons)
                    {
                        button.IsChecked = value;
                    }
                }

                OnPropertyChanged("IsChecked");
            }
        }
        public ObservableCollection<ViewButtonGroup> ButtonGroups { get; set; } = new ObservableCollection<ViewButtonGroup>();
    }
}
