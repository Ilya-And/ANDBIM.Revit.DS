using ANDBIM.Revit.DS.Model;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Dynamo.Applications;
using Dynamo.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ANDBIM.Revit.DS.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public ExternalCommandData CommandData;
        public string AssemblyPath { get; private set; }

        public object RDM {  get; private set; }

        public IDictionary<string, string> JournalDataDict { get; private set; } = new Dictionary<string, string>();

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
        public MainViewModel(List<object> nodes, ExternalCommandData commandData, object rdm, IDictionary<string, string> journalData)
        {
            CommandData = commandData;
            JournalDataDict = journalData;
            RDM = rdm;

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var arrayStr = thisAssemblyPath.Split('\\').ToList();
            arrayStr.Remove(arrayStr[arrayStr.Count - 1]);
            string folderPath = string.Join("\\", arrayStr.ToArray());
            AssemblyPath = folderPath;

            foreach (var node in nodes)
            {
                var nodeType = node.GetType();

                NodeWorker nodeWorker = new NodeWorker();
                nodeWorker.Node = node as Dynamo.Graph.Nodes.NodeModel;
                nodeWorker.Name = $"{nodeWorker.Node.Name}";//$"{node.GetType().GetProperty("Name")}_{node}:";

                // Определяем тип ноды и устанавливаем данные соответствующим образом
                if (nodeType.Name.Contains("CodeBlock"))
                {
                    object v = node.GetType().GetProperty("Code").GetValue(node, null);
                    nodeWorker.Value = v.ToString();
                }
                else if (nodeType.Name.Contains("Number") || nodeType.Name.Contains("Integer") || nodeType.Name.Contains("Double"))
                {
                    object v = node.GetType().GetProperty("Value").GetValue(node, null);
                    nodeWorker.Value = v.ToString();
                }
                else if (nodeType.Name.Contains("String"))
                {
                    object v = node.GetType().GetProperty("Value").GetValue(node, null);
                    nodeWorker.Value = v.ToString();
                }
                else if (nodeType.Name.Contains("Bool"))
                {
                    object v = node.GetType().GetProperty("Value").GetValue(node, null);

                    if (v.ToString().ToLower() == "true") nodeWorker.BoolValue = true;
                    else nodeWorker.BoolValue = false;

                    nodeWorker.IsBoolValue = true;
                }
                //else
                //{
                //    // Для неизвестных типов нодов пробуем универсальный подход
                //    SetGenericNodeData(node, commandData);
                //}

                NodeWorkerList.Add(nodeWorker);
            }
        }

        private ObservableCollection<NodeWorker> _nodeWorkerList { get; set; } = new ObservableCollection<NodeWorker>();
        public ObservableCollection<NodeWorker> NodeWorkerList
        {
            get => _nodeWorkerList;
            set
            {
                _nodeWorkerList = value; ;
                OnPropertyChanged("NodeWorkerList");
            }
        }

        private RelayCommand _startCommand;

        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand ?? new RelayCommand(obj =>
                {
                    SetInputNodesData(NodeWorkerList, CommandData);

                    var newRDM = RDM.GetType().GetMethod("ForceRun").Invoke(RDM, new object[] { });


                    DynamoRevitCommandData dynamoRevitCommandData = new DynamoRevitCommandData
                    {
                        Application = CommandData.Application,
                        JournalData = JournalDataDict
                    };

                    RaiseCloseRequest();

                    ResultWindow resultWindow = new ResultWindow();
                    resultWindow.ShowDialog();
                });
            }
        }

        private void SetCodeBlockNodeData(object node, ExternalCommandData commandData, string value)
        {
            try
            {
                // Для Code Block ноды устанавливаем код через свойство Code
                var codeProperty = node.GetType().GetProperty("Code");
                if (codeProperty != null)
                {
                    // Пример: устанавливаем код, который получает текущий документ
                    string code = "// Input from C#\\n" +
                                 $"doc = DocumentManager.Instance.CurrentDBDocument;\\n" +
                                 $"uidoc = DocumentManager.Instance.CurrentUIDocument;\\n" +
                                 $"app = DocumentManager.Instance.CurrentUIApplication.Application;";

                    codeProperty.SetValue(node, value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting CodeBlock data: {ex.Message}");
            }
        }

        /// <summary>
        /// Устанавливает данные для числовой ноды
        /// </summary>
        private void SetNumberNodeData(object node, string value)
        {
            try
            {
                // Пробуем различные свойства для установки значения
                var valueProperty = node.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    // Устанавливаем случайное число в качестве примера
                    Random rand = new Random();
                    valueProperty.SetValue(node, value);

                    return;
                }

                // Альтернативные свойства
                var numberProperty = node.GetType().GetProperty("Number");
                if (numberProperty != null)
                {
                    numberProperty.SetValue(node, value); // Пример значения
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting Number node data: {ex.Message}");
            }
        }

        /// <summary>
        /// Устанавливает данные для строковой ноды
        /// </summary>
        private void SetStringNodeData(object node, string value)
        {
            try
            {
                var valueProperty = node.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    valueProperty.SetValue(node, value);
                    return;
                }

                var textProperty = node.GetType().GetProperty("Text");
                if (textProperty != null)
                {
                    textProperty.SetValue(node, value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting String node data: {ex.Message}");
            }
        }

        /// <summary>
        /// Устанавливает данные для булевой ноды
        /// </summary>
        private void SetBooleanNodeData(object node, bool value)
        {
            try
            {
                var valueProperty = node.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    valueProperty.SetValue(node, value);
                    return;
                }

                var boolProperty = node.GetType().GetProperty("Bool");
                if (boolProperty != null)
                {
                    boolProperty.SetValue(node, value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting Boolean node data: {ex.Message}");
            }
        }

        /// <summary>
        /// Универсальный метод установки данных для ноды
        /// </summary>
        private void SetGenericNodeData(object node, ExternalCommandData commandData)
        {
            try
            {
                // Пробуем вызвать метод UpdateValue если он существует
                var updateMethod = node.GetType().GetMethod("UpdateValue");
                if (updateMethod != null)
                {
                    // Создаем данные для передачи
                    var updateData = new Dictionary<string, object>
                    {
                        { "commandData", commandData },
                        { "timestamp", DateTime.Now }
                    };

                    updateMethod.Invoke(node, new object[] { updateData });
                    return;
                }

                // Пробуем установить данные через свойство Data
                var dataProperty = node.GetType().GetProperty("Data");
                if (dataProperty != null)
                {
                    dataProperty.SetValue(node, commandData);
                    return;
                }

                // Последняя попытка: ищем любое свойство, которое можно установить
                var properties = node.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    if (property.CanWrite && property.PropertyType.IsClass)
                    {
                        try
                        {
                            property.SetValue(node, commandData);
                            break;
                        }
                        catch
                        {
                            // Продолжаем попытки с другими свойствами
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting generic node data: {ex.Message}");
            }
        }

        private void SetNodeData(NodeWorker node, ExternalCommandData commandData)
        {
            var nodeType = node.Node.GetType();

            // Определяем тип ноды и устанавливаем данные соответствующим образом
            if (nodeType.Name.Contains("CodeBlock"))
            {
                SetCodeBlockNodeData(node.Node, commandData, node.Value);
            }
            else if (nodeType.Name.Contains("Number") || nodeType.Name.Contains("Integer") || nodeType.Name.Contains("Double"))
            {
                SetNumberNodeData(node.Node, node.Value);
            }
            else if (nodeType.Name.Contains("String"))
            {
                SetStringNodeData(node.Node, node.Value);
            }
            else if (nodeType.Name.Contains("Bool"))
            {
                SetBooleanNodeData(node.Node, node.BoolValue);
            //}
            //else
            //{
            //    // Для неизвестных типов нодов пробуем универсальный подход
            //    SetGenericNodeData(node.Node, commandData);
            }
        }

        private void SetInputNodesData(ObservableCollection<NodeWorker> inputNodes, ExternalCommandData commandData)
        {
            foreach (var node in inputNodes)
            {
                try
                {
                    SetNodeData(node, commandData);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error setting data for node: {ex.Message}");
                }
            }
        }

        private List<object> GetInputNodes(DynamoModel dynamoModel)
        {
            var inputNodes = new List<object>();

            try
            {
                // Получаем текущее рабочее пространство
                var workspace = dynamoModel.CurrentWorkspace;
                if (workspace == null) return inputNodes;

                // Получаем все ноды в рабочем пространстве
                var nodes = workspace.Nodes;

                foreach (var node in nodes)
                {
                    // Пробуем получить свойство IsInput через рефлексию
                    var isInputProperty = node.GetType().GetProperty("IsInputNode");
                    if (isInputProperty != null)
                    {
                        var isInputValue = isInputProperty.GetValue(node);
                        if (isInputValue is bool && (bool)isInputValue)
                        {
                            inputNodes.Add(node);
                        }
                    }

                    // Дополнительная проверка для определенных типов нодов
                    var nodeType = node.GetType();
                    if (nodeType.Name.Contains("Input") || nodeType.Name.Contains("CodeBlock"))
                    {
                        // Для Code Block нодов проверяем другие свойства
                        var hasInputProperty = nodeType.GetProperty("HasInput");
                        if (hasInputProperty != null)
                        {
                            var hasInputValue = hasInputProperty.GetValue(node);
                            if (hasInputValue is bool && (bool)hasInputValue)
                            {
                                inputNodes.Add(node);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем выполнение
                System.Diagnostics.Debug.WriteLine($"Error getting input nodes: {ex.Message}");
            }

            return inputNodes;
        }

        private DynamoModel GetDynamoModel(object revitDynamoModel)
        {
            try
            {
                // Пробуем разные возможные свойства, содержащие модель Dynamo
                var properties = revitDynamoModel.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(DynamoModel) ||
                        property.PropertyType.IsSubclassOf(typeof(DynamoModel)))
                    {
                        return property.GetValue(revitDynamoModel) as DynamoModel;
                    }
                }

                // Если не нашли напрямую, ищем через методы
                var methods = revitDynamoModel.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    if (method.Name.Contains("GetModel") && method.ReturnType == typeof(DynamoModel))
                    {
                        return method.Invoke(revitDynamoModel, null) as DynamoModel;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
