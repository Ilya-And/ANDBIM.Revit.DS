using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ANDBIM.Revit.DS
{
    public class ViewTab
    {
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
        public ObservableCollection<ViewPanel> Panels { get; set; } = new ObservableCollection<ViewPanel>();

    }
}
