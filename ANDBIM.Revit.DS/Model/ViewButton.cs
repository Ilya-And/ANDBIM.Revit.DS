using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ANDBIM.Revit.DS
{
    public class ViewButton : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
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
        private bool _isChecked { get; set; } = false;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;

                OnPropertyChanged("IsChecked");
            }
        }
    }
}
