using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANDBIM.Revit.DS
{
    public class ViewPanel
    {
        public ViewPanel(string name, ObservableCollection<ViewButtonGroup> buttonGroups)
        {
            Name = name;
            ButtonGroups = buttonGroups;
        }
        public ViewPanel()
        {
        }
        public string Name { get; set; }
        public ObservableCollection<ViewButtonGroup> ButtonGroups { get; set; }
    }
}
