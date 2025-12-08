using ANDBIM.Revit.DS.Model;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Dynamo.Applications;
using Dynamo.Applications.Models;
using Dynamo.Models;
using Microsoft.Win32;
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
using System.Windows.Media.Imaging;

namespace ANDBIM.Revit.DS
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class DynamoStartCommand : IExternalCommand
    {
        public static string StartedButtonName = "";
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            try
            {
                // Получаем путь к скрипту из описания кнопки
                var uiApp = commandData.Application;
                var activeId = uiApp.ActiveAddInId;

                // Получаем кнопку которая вызвала команду
                //var button = GetButtonItem("Dyno Scripts", "Dynamic Scripts", StartedButtonName);
                //MessageBox.Show($"{button.Name}");
                //if (button == null)
                //{
                //    message = "Cannot determine which script to run";
                //    return Result.Failed;
                //}

                string scriptPath = ANDBIMApplication.ScriptToRun;

                //foreach (string buttonName in EGPApplication.CommandsDictionary.Keys)
                //{
                //    foreach (string path in EGPApplication.CommandsDictionary[buttonName].Keys)
                //    {
                //        if (EGPApplication.CommandsDictionary[buttonName][path] == this.GetType().Name)
                //        {
                //            scriptPath = path;
                //        }
                //    }
                //}

                //var dsad = commandData.Application.ActiveAddInId;

                if (!File.Exists(scriptPath))
                {
                    //message = $"Script file not found: {scriptPath}";

                    var result = MessageBox.Show("Скрипт не найден!\nХотите обновить ссылку на скрипт?", "Ошибка", MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        var openFileDialog = new OpenFileDialog();
                        var resultOpenFile = openFileDialog.ShowDialog();

                        if (resultOpenFile == true && openFileDialog.FileName != null
                        && openFileDialog.FileName.Contains(".dyn"))
                        {
                            var configList = JsonConvert.DeserializeObject<List<ViewTab>>(File.ReadAllText($"{ANDBIMApplication.AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json"));

                            foreach (ViewTab viewTab in configList)
                            {
                                foreach (ViewPanel panel in viewTab.Panels)
                                {
                                    foreach (ViewButtonGroup viewButtonGroup in panel.ButtonGroups)
                                    {
                                        foreach (ViewButton viewButton in viewButtonGroup.Buttons)
                                        {
                                            if (viewButton.Name == ANDBIMApplication.ButtonName)
                                            {
                                                viewButton.ScriptPath = viewButton.ScriptPath;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            File.WriteAllText($"{ANDBIMApplication.AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json", JsonConvert.SerializeObject(configList));
                        }
                    }

                    return Result.Failed;
                }

                string text = File.ReadAllText(scriptPath);
                text = text.Replace(@"""RunType"": ""Manual"",", @"""RunType"": ""Automatic"",");

                File.WriteAllText(scriptPath, text);

                IDictionary<string, string> journalData = new Dictionary<string, string>
                {
                    {JournalKeys.ShowUiKey, false.ToString()},
                    {JournalKeys.AutomationModeKey, true.ToString()},
                    {"dynPath", scriptPath},
                    //{JournalKeys.DynPathKey, dynamoJournal},
                    //{JournalKeys.DynPathExecuteKey, true.ToString()},
                    {JournalKeys.ForceManualRunKey, true.ToString()},
                    {JournalKeys.ModelShutDownKey, true.ToString()},
                    //{JournalKeys.ModelNodesInfo, false.ToString()},
                };

                //get all loaded assemblies
                var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

                //find the Dynamo Revit one
                var dynamoRevitApplication = loadedAssemblies
                    .FirstOrDefault(a => a.FullName.Contains("DynamoRevitDS"));

                //create our own instances of these things using reflection. Shoutout to BirdTools for helping with this.
                object dInst = dynamoRevitApplication.CreateInstance("Dynamo.Applications.DynamoRevit");
                object dta = dynamoRevitApplication.CreateInstance("Dynamo.Applications.DynamoRevitCommandData");
                dta.GetType().GetProperty("Application").SetValue(dta, uiApp, null);
                dta.GetType().GetProperty("JournalData").SetValue(dta, journalData, null);

                object[] parameters = new object[] { dta };

                dInst.GetType().GetMethod("ExecuteCommand").Invoke(dInst, parameters);

                object rdm = dInst.GetType().GetProperty("RevitDynamoModel").GetValue(dInst, null);

                // Получаем модель Dynamo
                DynamoModel dynamoModel = rdm as DynamoModel;
                if (dynamoModel == null)
                {
                    // Если не удалось получить напрямую, пробуем через рефлексию
                    dynamoModel = GetDynamoModel(rdm);
                }

                if (dynamoModel != null)
                {
                    // Получаем все ноды с IsInputNode == true
                    var inputNodes = GetInputNodes(dynamoModel);

                    if (inputNodes != null && inputNodes.Count > 0)
                    {
                        MainWindow mainWindow = new MainWindow(inputNodes, commandData, rdm, journalData);
                        mainWindow.ShowDialog();
                    }


                    // Передаем данные на входные ноды
                    //SetInputNodesData(inputNodes, commandData);
                }

                //rdm.GetType().GetMethod("ForceRun").Invoke(rdm, new object[] { });


                //DynamoRevitCommandData dynamoRevitCommandData = new DynamoRevitCommandData
                //{
                //    Application = uiApp,
                //    JournalData = journalData
                //};



                //Packages.ResolvePackages(DynamoRevit.RevitDynamoModel.PathManager.DefaultPackagesDirectory, scriptPath);

                return Result.Succeeded;

                //// Запускаем скрипт через рефлексию
                //string Journal_Dynamo_Path = scriptPath;
                //DynamoRevit dynamoRevit = new DynamoRevit();
                //DynamoRevitCommandData dynamoRevitCommandData = new DynamoRevitCommandData();
                //dynamoRevitCommandData.Application = commandData.Application;
                //IDictionary<string, string> journalData = new Dictionary<string, string>
                //{
                //    { Dynamo.Applications.JournalKeys.ShowUiKey, false.ToString() }, // don't show DynamoUI at runtime
                //    { Dynamo.Applications.JournalKeys.AutomationModeKey, true.ToString() }, //run journal automatically
                //    { Dynamo.Applications.JournalKeys.DynPathKey, Journal_Dynamo_Path }, //run node at this file path
                //    { Dynamo.Applications.JournalKeys.DynPathExecuteKey, true.ToString() }, // The journal file can specify if the Dynamo workspace opened from DynPathKey will be executed or not. If we are in automation mode the workspace will be executed regardless of this key.
                //    { Dynamo.Applications.JournalKeys.ForceManualRunKey, false.ToString() }, // don't run in manual mode
                //    { Dynamo.Applications.JournalKeys.ModelShutDownKey, true.ToString() },
                //    { Dynamo.Applications.JournalKeys.ModelNodesInfo, false.ToString() }

                //};

                //dynamoRevitCommandData.JournalData = journalData;
                //return dynamoRevit.ExecuteCommand(dynamoRevitCommandData);


            }
            catch (Exception ex)
            {
                //message = ex.Message;
                Autodesk.Revit.UI.TaskDialog.Show("Error", $"Failed to run script: {ex.Message}");
                return Result.Failed;
            }
        }

        private void SetCodeBlockNodeData(object node, ExternalCommandData commandData)
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

                    codeProperty.SetValue(node, code);
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
        private void SetNumberNodeData(object node)
        {
            try
            {
                // Пробуем различные свойства для установки значения
                var valueProperty = node.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    // Устанавливаем случайное число в качестве примера
                    Random rand = new Random();
                    valueProperty.SetValue(node, rand.Next(1, 100).ToString());

                    return;
                }

                // Альтернативные свойства
                var numberProperty = node.GetType().GetProperty("Number");
                if (numberProperty != null)
                {
                    numberProperty.SetValue(node, 42.0); // Пример значения
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
        private void SetStringNodeData(object node)
        {
            try
            {
                var valueProperty = node.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    valueProperty.SetValue(node, "Data from C# plugin");
                    return;
                }

                var textProperty = node.GetType().GetProperty("Text");
                if (textProperty != null)
                {
                    textProperty.SetValue(node, "Input from external command");
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
        private void SetBooleanNodeData(object node)
        {
            try
            {
                var valueProperty = node.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    valueProperty.SetValue(node, true);
                    return;
                }

                var boolProperty = node.GetType().GetProperty("Boolean");
                if (boolProperty != null)
                {
                    boolProperty.SetValue(node, true);
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

        private void SetNodeData(object node, ExternalCommandData commandData)
        {
            var nodeType = node.GetType();

            // Определяем тип ноды и устанавливаем данные соответствующим образом
            if (nodeType.Name.Contains("CodeBlock"))
            {
                SetCodeBlockNodeData(node, commandData);
            }
            else if (nodeType.Name.Contains("Number") || nodeType.Name.Contains("Integer") || nodeType.Name.Contains("Double"))
            {
                SetNumberNodeData(node);
            }
            else if (nodeType.Name.Contains("String"))
            {
                SetStringNodeData(node);
            }
            else if (nodeType.Name.Contains("Boolean"))
            {
                SetBooleanNodeData(node);
            //}
            //else
            //{
            //    // Для неизвестных типов нодов пробуем универсальный подход
            //    SetGenericNodeData(node, commandData);
            }
        }

        private void SetInputNodesData(List<object> inputNodes, ExternalCommandData commandData)
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
                    var nodeType = node.GetType();

                    if (nodeType.Name.Contains("CodeBlock") 
                        || nodeType.Name.Contains("Number") 
                        || nodeType.Name.Contains("Integer") 
                        || nodeType.Name.Contains("Double")
                        || nodeType.Name.Contains("String")
                        || nodeType.Name.Contains("Boolean"))
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

        private Autodesk.Revit.UI.RibbonButton GetCallingButton(UIApplication uiApp)
        {
            // Получаем текущую панель и ищем активную кнопку
            // Это упрощенный подход - в реальности может потребоваться более сложная логика
            var ribbonControl = uiApp.GetRibbonPanels().FirstOrDefault();
            return ribbonControl?.GetItems().FirstOrDefault(i => i is Autodesk.Revit.UI.RibbonButton) as Autodesk.Revit.UI.RibbonButton;
        }

        //private Autodesk.Windows.RibbonItem GetButtonItem(string tabName, string panelName, string itemName)
        //{
        //    Autodesk.Windows.RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;
        //    foreach (Autodesk.Windows.RibbonTab tab in ribbon.Tabs)
        //    {
        //        if (tab.Name == tabName)
        //        {
        //            foreach (Autodesk.Windows.RibbonPanel panel in tab.Panels)
        //            {
        //                if (panel.Source.Title == panelName)
        //                {
        //                    return panel.FindItem("CustomCtrl_%CustomCtrl_%"
        //                      + tabName + "%" + panelName + "%" + itemName,
        //                      true) as Autodesk.Windows.RibbonItem;
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}

        private Autodesk.Windows.RibbonButton GetButtonItem(string tabName, string panelName, string itemName)
        {
            RibbonControl ribbon = ComponentManager.Ribbon;
            foreach (Autodesk.Windows.RibbonTab tab in ribbon.Tabs)
            {
                if (tab.Name == tabName)
                {
                    foreach (Autodesk.Windows.RibbonPanel panel in tab.Panels)
                    {
                        if (panelName == panel.AutomationName)
                        {
                            return panel.FindItem("CustomCtrl_%CustomCtrl_%"
                              + tabName + "%" + panelName + "%" + itemName,
                              true) as Autodesk.Windows.RibbonButton;
                        }

                        //return panel.FindItem("CustomCtrl_%CustomCtrl_%"
                        //  + tabName + "%" + panelName + "%" + itemName,
                        //  true) as Autodesk.Windows.RibbonItem;
                    }
                }
            }
            return null;
        }

        private Result RunDynamoScript(UIApplication uiApp, string scriptPath, ref string message)
        {
            try
            {
                // Загружаем сборки Dynamo через рефлексию
                var dynamoRevitType = FindDynamoRevitType();
                if (dynamoRevitType == null)
                {
                    message = "DynamoRevit type not found. Make sure Dynamo for Revit is installed.";
                    return Result.Failed;
                }

                // Создаем экземпляр DynamoRevit
                var dynamoRevit = Activator.CreateInstance(dynamoRevitType);

                // Получаем метод ExecuteCommand
                var executeMethod = dynamoRevitType.GetMethod("ExecuteCommand",
                    new[] {
                        typeof(UIApplication),
                        typeof(UIDocument),
                        typeof(Document),
                        typeof(string)
                    });

                if (executeMethod == null)
                {
                    message = "ExecuteCommand method not found in DynamoRevit";
                    return Result.Failed;
                }

                // Вызываем метод
                executeMethod.Invoke(dynamoRevit, new object[] {
                    uiApp,
                    uiApp.ActiveUIDocument,
                    uiApp.ActiveUIDocument.Document,
                    scriptPath
                });

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = $"Failed to execute Dynamo script: {ex.InnerException?.Message ?? ex.Message}";
                return Result.Failed;
            }
        }

        private Type FindDynamoRevitType()
        {
            // Ищем тип в загруженных сборках
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in loadedAssemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    var dynamoType = types.FirstOrDefault(t =>
                        t.Name == "DynamoRevit" &&
                        t.GetMethod("ExecuteCommand") != null);

                    if (dynamoType != null)
                        return dynamoType;
                }
                catch
                {
                    // Пропускаем сборки которые нельзя проанализировать
                    continue;
                }
            }
            return null;
        }
    }
}
