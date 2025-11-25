using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANDBIM.Revit.DS
{
    public class ViewButtonGroup
    {
        public ViewButtonGroup(ViewButtonGroupTypeEnum buttonGroupType, ObservableCollection<ViewButton> buttons)
        {
            ButtonGroupType = buttonGroupType;
            Buttons = buttons;
        }
        public ViewButtonGroupTypeEnum ButtonGroupType { get; set; } = ViewButtonGroupTypeEnum.StandartButton;
        public ObservableCollection<ViewButton> Buttons { get; set; } = new ObservableCollection<ViewButton>();
    }
}
