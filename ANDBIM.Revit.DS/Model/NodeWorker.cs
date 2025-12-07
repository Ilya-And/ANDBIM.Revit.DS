using System;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

        private Dynamo.Graph.Nodes.NodeModel _node { get; set; }
        public Dynamo.Graph.Nodes.NodeModel Node
        {
            get => _node;
            set
            {
                _node = value;
            }
        }
    }
}
