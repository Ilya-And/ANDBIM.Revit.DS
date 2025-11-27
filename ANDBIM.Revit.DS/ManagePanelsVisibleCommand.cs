using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Dynamo.Applications;
using Dynamo.Applications.Models;
using Dynamo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ANDBIM.Revit.DS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ManagePanelsVisibleCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            // Access the RibbonControl using reflection
            RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;

            foreach (RibbonTab tab in ribbon.Tabs)
            {
                foreach (Autodesk.Windows.RibbonPanel ribbonPanel in tab.Panels)
                {
                    if (ribbonPanel.Source.AutomationName == "ANDBIM.DS.Settings")
                    {
                        try
                        {
                            if (ribbonPanel.IsVisible == true) ribbonPanel.IsVisible = false;
                            else ribbonPanel.IsVisible = true;
                        }
                        catch { }
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
