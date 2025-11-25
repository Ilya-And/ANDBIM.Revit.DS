using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANDBIM.Revit.DS
{
    public class ViewButton
    {
        public ViewButton(string imagePath, string scriptPath, string description, string longDescription, string name)
        {
            ImagePath = imagePath;
            ScriptPath = scriptPath;
            Description = description;
            LongDescription = longDescription;
            Name = name;
        }
        public string ImagePath { get; set; } = "";
        public string ScriptPath { get; set; } = "";
        public string Description { get; set; } = "";
        public string LongDescription { get; set; } = "";
        public string Name { get; set; } = "";
    }
}
