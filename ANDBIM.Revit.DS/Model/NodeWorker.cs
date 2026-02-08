using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ANDBIM.Revit.DS.Model
{
    public class NodeWorker
    {
        public NodeWorker()
        {
        }
        public NodeWorker(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public NodeWorker(string name, bool boolValue)
        {
            Name = name;
            BoolValue = boolValue;
        }

        private string _name { get; set; } = "";
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
            }
        }

        private string _value { get; set; } = "";
        public string Value
        {
            get => _value;
            set
            {
                _value = value;
            }
        }

        private bool _boolValue { get; set; } = false;
        public bool BoolValue
        {
            get => _boolValue;
            set
            {
                _boolValue = value;
                if (value) Value = "True";
                else Value = "False";
            }
        }

        private bool _notBoolValue { get; set; } = false;
        public bool NotBoolValue
        {
            get => _notBoolValue;
            set
            {
                _notBoolValue = value;
                if (value) Value = "False";
                else Value = "True";
            }
        }

        private bool _isBoolValue { get; set; } = false;
        public bool IsBoolValue
        {
            get => _isBoolValue;
            set
            {
                _isBoolValue = value;

                if (value)
                {
                    BoolValueVisibility = Visibility.Visible;
                    TextValueVisibility = Visibility.Hidden;
                }
                    
                else
                {
                    BoolValueVisibility = Visibility.Hidden;
                    TextValueVisibility = Visibility.Visible;
                }
            }
        }

        private Dynamo.Graph.Nodes.NodeModel _node { get; set; }
        public Dynamo.Graph.Nodes.NodeModel Node
        {
            get => _node;
            set
            {
                _node = value;
            }
        }

        private Visibility _textValueVisibility { get; set; } = Visibility.Visible;
        public Visibility TextValueVisibility
        {
            get => _textValueVisibility;
            set
            {
                _textValueVisibility = value;
            }
        }

        private Visibility _boolValueVisibility { get; set; } = Visibility.Hidden;
        public Visibility BoolValueVisibility
        {
            get => _boolValueVisibility;
            set
            {
                _boolValueVisibility = value;
            }
        }
    }
}
