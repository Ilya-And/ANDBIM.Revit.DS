using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Dynamo.Applications;
using Dynamo.Applications.Models;
using Dynamo.Models;

namespace ANDBIM.Revit.DS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class RemoveElementCommand : IExternalCommand
    {
        public static string StartedButtonName = "";
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            RemoveElementWindow removeElementWindow = new RemoveElementWindow(commandData);
            removeElementWindow.ShowDialog();

            return Result.Succeeded;
        }

        
    }
}
