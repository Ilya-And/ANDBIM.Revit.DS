using ANDBIM.Revit.DS.ViewModel;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ANDBIM.Revit.DS
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class ResultNodesWindow : Window
    {
        public ResultNodesWindow(ExternalCommandData commandData, object rdm, IDictionary<string, string> journalData)
        {
            InitializeComponent();

            var vm = new ResultViewModel(commandData, rdm, journalData);
            vm.CloseRequest += (s, e) => Close();

            DataContext = vm;

            //foreach (var node in nodes)
            //{
            //    var nodeType = node.GetType();

            //    // Определяем тип ноды и устанавливаем данные соответствующим образом
            //    if (nodeType.Name.Contains("CodeBlock"))
            //    {
            //        TextBlock textBlock = new TextBlock();
            //        System.Windows.Controls.TextBox textBox = new System.Windows.Controls.TextBox();

            //        textBlock.Text = $"{node.GetType().GetProperty("Name")}:";
            //        object v = node.GetType().GetProperty("Code").GetValue(node, null);
            //        textBox.Text = v.ToString();

            //        stack.Children.Add(textBlock);
            //        stack.Children.Add(textBox);
            //    }
            //    else if (nodeType.Name.Contains("Number") || nodeType.Name.Contains("Integer") || nodeType.Name.Contains("Double"))
            //    {
            //        TextBlock textBlock = new TextBlock();
            //        System.Windows.Controls.TextBox textBox = new System.Windows.Controls.TextBox();

            //        textBlock.Text = $"{node.GetType().GetProperty("Name")}:";
            //        object v = node.GetType().GetProperty("Value").GetValue(node, null);
            //        textBox.Text = v.ToString();

            //        stack.Children.Add(textBlock);
            //        stack.Children.Add(textBox);
            //    }
            //    else if (nodeType.Name.Contains("String"))
            //    {
            //        TextBlock textBlock = new TextBlock();
            //        System.Windows.Controls.TextBox textBox = new System.Windows.Controls.TextBox();

            //        textBlock.Text = $"{node.GetType().GetProperty("Name")}:";
            //        object v = node.GetType().GetProperty("Value").GetValue(node, null);
            //        textBox.Text = v.ToString();

            //        stack.Children.Add(textBlock);
            //        stack.Children.Add(textBox);
            //    }
            //    //else if (nodeType.Name.Contains("Boolean"))
            //    //{
            //    //    SetBooleanNodeData(node);
            //    //}
            //    //else
            //    //{
            //    //    // Для неизвестных типов нодов пробуем универсальный подход
            //    //    SetGenericNodeData(node, commandData);
            //    //}
            //}
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
