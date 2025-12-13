using ANDBIM.Revit.DS.Model;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration.Assemblies;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace ANDBIM.Revit.DS
{
    public class ANDBIMApplication : IExternalApplication
    {
        //private static string ScriptsFolder = "D:\\Архив\\Scripts";
        public static string ScriptToRun = "";
        public static string ButtonName = "";
        public static string TabRun = "";
        public static string PanelRun = "";
        public static List<ViewTab> CommandsList = new List<ViewTab>();
        public static UIControlledApplication ThisApplication;

        public static string AssemblyPath { get; private set; }

        public Result OnStartup(UIControlledApplication application)
        {
            ThisApplication = application;
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var arrayStr = thisAssemblyPath.Split('\\').ToList();
            arrayStr.Remove(arrayStr[arrayStr.Count - 1]);
            string folderPath = string.Join("\\", arrayStr.ToArray());
            AssemblyPath = folderPath;

            try
            {
                //// Создание папки если не существует
                //Directory.CreateDirectory(ScriptsFolder);

                //// Создание вкладки Ribbon
                //string tabName = "Dyno Scripts";
                //try
                //{
                //    application.CreateRibbonTab(tabName);
                //}
                //catch (Exception) { }

                //// Создание панели
                //Autodesk.Revit.UI.RibbonPanel panel = application.CreateRibbonPanel(tabName, "Dynamic Scripts");

                //// Загрузка скриптов
                //LoadScripts(panel);


                var andbimPanel = application.CreateRibbonPanel("ANDBIM DS");

                PushButtonData buttonDataAddTab = new PushButtonData("Создать вкладку",
                                                                        "Создать\nвкладку",
                                                                        thisAssemblyPath,
                                                                        "ANDBIM.Revit.DS.RibbonTabCreatorCommand");
                PushButtonData buttonDataManagePanelVisible = new PushButtonData("Панели\nуправления",
                                                                        "Панели\nуправления",
                                                                        thisAssemblyPath,
                                                                        "ANDBIM.Revit.DS.ManagePanelsVisibleCommand");

                PushButton newButtonAddTab = andbimPanel.AddItem(buttonDataAddTab) as PushButton;
                newButtonAddTab.ToolTip = "Создание пользовательской вкладки";
                BitmapImage newButtonAddTabImg16 = new BitmapImage(new Uri($"pack://application:,,,/ANDBIM.Revit.DS;component/Resources/CreateTab_16.ico"));
                BitmapImage newButtonAddTabImg = new BitmapImage(new Uri($"pack://application:,,,/ANDBIM.Revit.DS;component/Resources/CreateTab.ico"));
                
                newButtonAddTab.LargeImage = newButtonAddTabImg;
                newButtonAddTab.Image = newButtonAddTabImg16;

                PushButton newButtonManagePanelVisible = andbimPanel.AddItem(buttonDataManagePanelVisible) as PushButton;
                newButtonManagePanelVisible.ToolTip = "Включение/отключение панелей управления на созданных вкладках";
                BitmapImage newButtonManagePanelVisibleImg16 = new BitmapImage(new Uri($"pack://application:,,,/ANDBIM.Revit.DS;component/Resources/ManagePanels_16.ico"));
                BitmapImage newButtonManagePanelVisibleImg = new BitmapImage(new Uri($"pack://application:,,,/ANDBIM.Revit.DS;component/Resources/ManagePanels.ico"));

                newButtonManagePanelVisible.LargeImage = newButtonManagePanelVisibleImg;
                newButtonManagePanelVisible.Image = newButtonManagePanelVisibleImg16;

                bool isExist = File.Exists($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json");

                if (!isExist)
                {
                    File.Create($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json");
                }
                else
                {
                    var configList = JsonConvert.DeserializeObject<List<ViewTab>>(File.ReadAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json"));

                    foreach (ViewTab viewTab in configList)
                    {
                        ThisApplication.CreateRibbonTab(viewTab.Name);

                        var settingsPanel = ThisApplication.CreateRibbonPanel(viewTab.Name, "ANDBIM.DS.Settings");

                        PushButtonData buttonDataAddButton = new PushButtonData("Создать кнопку",
                                                                        "Создать кнопку",
                                                                        Assembly.GetExecutingAssembly().Location,
                                                                        "ANDBIM.Revit.DS.RibbonButtonCreatorCommand");
                        PushButtonData buttonDataAddPanel = new PushButtonData("Создать панель",
                                                                                "Создать панель",
                                                                                Assembly.GetExecutingAssembly().Location,
                                                                                "ANDBIM.Revit.DS.RibbonPanelCreatorCommand");

                        BitmapImage buttonDataAddButtonImg16 = new BitmapImage(new Uri($"pack://application:,,,/ANDBIM.Revit.DS;component/Resources/CreateButton_16.ico"));
                        BitmapImage buttonDataAddButtonImg = new BitmapImage(new Uri($"pack://application:,,,/ANDBIM.Revit.DS;component/Resources/CreateButton.ico"));

                        buttonDataAddButton.LargeImage = buttonDataAddButtonImg;
                        buttonDataAddButton.Image = buttonDataAddButtonImg16;

                        BitmapImage buttonDataAddPanelImg16 = new BitmapImage(new Uri($"pack://application:,,,/ANDBIM.Revit.DS;component/Resources/CreatePanel_16.ico"));
                        BitmapImage buttonDataAddPanelImg = new BitmapImage(new Uri($"pack://application:,,,/ANDBIM.Revit.DS;component/Resources/CreatePanel.ico"));

                        buttonDataAddPanel.LargeImage = buttonDataAddPanelImg;
                        buttonDataAddPanel.Image = buttonDataAddPanelImg16;

                        settingsPanel.AddItem(buttonDataAddButton);
                        settingsPanel.AddItem(buttonDataAddPanel);

                        settingsPanel.Visible = false;

                        foreach (ViewPanel panel in viewTab.Panels)
                        {
                            var ribbonPanel = ThisApplication.CreateRibbonPanel(viewTab.Name, panel.Name);

                            foreach (ViewButtonGroup viewButtonGroup in panel.ButtonGroups)
                            {
                                if (viewButtonGroup.ButtonGroupType == ViewButtonGroupTypeEnum.StandartButton)
                                {
                                    string imagePath = viewButtonGroup.Buttons[0].ImagePath;

                                    PushButtonData buttonData = new PushButtonData($"{viewButtonGroup.Buttons[0].Name}",
                                                                    $"{viewButtonGroup.Buttons[0].Name}",
                                                                    thisAssemblyPath,
                                                                    "ANDBIM.Revit.DS.DynamoStartCommand");
                                    try
                                    {
                                        //BitmapImage imgOriginal = new BitmapImage(new Uri(imagePath));

                                        buttonData.Image = ImageWorker.ResizeBitmapImage(16, 16, imagePath);

                                        Thread.Sleep(1000);

                                        buttonData.LargeImage = ImageWorker.ResizeBitmapImage(32, 32, imagePath);
                                    }
                                    catch (Exception ex)
                                    {
                                        //MessageBox.Show($"{ex.Message}\n{ex.InnerException}\n{ex.StackTrace}");
                                    }

                                    PushButton newRibbonButton = ribbonPanel.AddItem(buttonData) as PushButton;

                                    newRibbonButton.ToolTip = viewButtonGroup.Buttons[0].Description;
                                    newRibbonButton.LongDescription = viewButtonGroup.Buttons[0].LongDescription;
                                }

                                else
                                {
                                    PushButtonData pushButtonData1 = new PushButtonData($"{viewButtonGroup.Buttons[0].Name}",
                                                                    $"{viewButtonGroup.Buttons[0].Name}",
                                                                    thisAssemblyPath,
                                                                    "ANDBIM.Revit.DS.DynamoStartCommand");
                                    PushButtonData pushButtonData2 = new PushButtonData($"{viewButtonGroup.Buttons[1].Name}",
                                                                    $"{viewButtonGroup.Buttons[1].Name}",
                                                                    thisAssemblyPath,
                                                                    "ANDBIM.Revit.DS.DynamoStartCommand");
                                    PushButtonData pushButtonData3 = new PushButtonData($"{viewButtonGroup.Buttons[2].Name}",
                                                                    $"{viewButtonGroup.Buttons[2].Name}",
                                                                    thisAssemblyPath,
                                                                    "ANDBIM.Revit.DS.DynamoStartCommand");

                                    ribbonPanel.AddStackedItems(pushButtonData1, pushButtonData2, pushButtonData3);
                                }
                            }
                        }
                    }
                }

                Autodesk.Windows.ComponentManager.UIElementActivated += ComponentManagerOnUIElementActivated;

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Autodesk.Revit.UI.TaskDialog.Show("Error", $"Failed to initialize Dyno Manager: {ex.Message}");
                return Result.Failed;
            }

            //application.ControlledApplication.ApplicationInitialized += OnApplicationInitialized;
        }

        private Autodesk.Windows.RibbonButton GetButtonItem(string tabName, string panelName)
        {
            RibbonControl ribbon = ComponentManager.Ribbon;
            foreach (Autodesk.Windows.RibbonTab tab in ribbon.Tabs)
            {
                if (tab.Name == tabName)
                {
                    foreach (Autodesk.Windows.RibbonPanel panel in tab.Panels)
                    {
                        RibbonItemCollection items = panel.Source.Items;

                        foreach (Autodesk.Windows.RibbonItem item in items)
                        {
                            if (item is Autodesk.Windows.RibbonButton)
                            {
                                ((Autodesk.Windows.RibbonButton)item).PropertyChanged += (sender, e) =>
                                {
                                    MessageBox.Show(e.PropertyName);
                                };
                            }
                        }

                        //return panel.FindItem("CustomCtrl_%CustomCtrl_%"
                        //  + tabName + "%" + panelName + "%" + itemName,
                        //  true) as Autodesk.Windows.RibbonItem;
                    }
                }
            }
            return null;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            //application.ControlledApplication.ApplicationInitialized -= OnApplicationInitialized;

            Autodesk.Windows.ComponentManager.UIElementActivated -= ComponentManagerOnUIElementActivated;
            return Result.Succeeded;
        }

        private void ComponentManagerOnUIElementActivated(object sender, UIElementActivatedEventArgs e)
        {
            if (e.Item is null) return;
            try
            {
                ButtonName = e.Item.AutomationName;

                var configList = JsonConvert.DeserializeObject<List<ViewTab>>(File.ReadAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json"));

                foreach (ViewTab viewTab in configList)
                {
                    foreach (ViewPanel panel in viewTab.Panels)
                    {
                        foreach (ViewButtonGroup viewButtonGroup in panel.ButtonGroups)
                        {
                            foreach (ViewButton viewButton in viewButtonGroup.Buttons)
                            {
                                if (viewButton.Name == e.Item.AutomationName)
                                {
                                    ScriptToRun = viewButton.ScriptPath;
                                    break;
                                }
                            }
                        }
                    }
                }
                //ScriptToRun = $"{AssemblyPath}//{e.Item.AutomationName}.dyn";
            }
            catch (Exception)
            {
                // suppress the error if it happens
            }
        }


        private void LoadScripts(Autodesk.Revit.UI.RibbonPanel panel)
        {
            // Поиск всех .dyn файлов
            var dynFiles = Directory.GetFiles("ScriptsFolder", "*.dyn");

            string path = "";

            int i = 1;

            foreach (var dynFile in dynFiles)
            {
                try
                {
                    string scriptName = System.IO.Path.GetFileNameWithoutExtension(dynFile);
                    path += $"\n{scriptName} ({dynFile})";
                    CreateButton(panel, scriptName, dynFile, i);

                }
                catch (Exception ex)
                {
                    Autodesk.Revit.UI.TaskDialog.Show("Error", $"Failed to create button for {System.IO.Path.GetFileName(dynFile)}: {ex.Message}");
                }
                i++;
            }

            MessageBox.Show(path);
        }

        private PushButton AddButton(Autodesk.Revit.UI.RibbonPanel ribbonPanel, string buttonName, string path, string linkToCommand, string toolTip, string imageName = "LOGO_EGP.ico", bool isZeroDoc = false)
        {
            PushButtonData buttonData = new PushButtonData(
               buttonName,
               buttonName,
               path,
               linkToCommand);

            //Делаем инструмент доступным без открытых документов, при открытом пустом Revit только

            if (isZeroDoc)
            {
                buttonData.AvailabilityClassName = linkToCommand.Replace("StartCommand", "Availability");
            }


            BitmapImage img = new BitmapImage(new Uri($"pack://application:,,,/EGPPlugin.Revit;component/Resources/{imageName}"));
            buttonData.LargeImage = img;

            buttonData.Image = img;
            PushButton newButton = ribbonPanel.AddItem(buttonData) as PushButton;
            newButton.ToolTip = toolTip;

            return newButton;
        }

        private static PushButtonData CreateButtonData(string buttonName, string path, string linkToCommand, string toolTip, string imageName = "LOGO_EGP.ico", bool isZeroDoc = false)
        {
            PushButtonData buttonData = new PushButtonData(
                           buttonName,
                           buttonName,
                           path,
                           linkToCommand);

            //Делаем инструмент доступным без открытых документов, при открытом пустом Revit только

            if (isZeroDoc)
            {
                buttonData.AvailabilityClassName = linkToCommand.Replace("StartCommand", "Availability");
            }

            BitmapImage img = new BitmapImage(new Uri($"pack://application:,,,/EGPPlugin.Revit;component/Resources/{imageName}"));
            BitmapImage img16 = new BitmapImage(new Uri($"pack://application:,,,/EGPPlugin.Revit;component/Resources/{imageName.Replace(".ico", "_16.ico")}"));

            buttonData.LargeImage = img;
            buttonData.Image = img16;
            buttonData.ToolTip = toolTip;

            return buttonData;
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

        private Autodesk.Windows.RibbonPanel GetRibbonPanel(string tabName, string panelName)
        {
            Autodesk.Windows.RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;
            foreach (Autodesk.Windows.RibbonTab tab in ribbon.Tabs)
            {
                if (tab.Name == tabName)
                {
                    foreach (Autodesk.Windows.RibbonPanel panel in tab.Panels)
                    {
                        if (panel.Source.Title == panelName)
                        {
                            return panel;
                        }
                    }
                }
            }
            return null;
        }



        private void ClearOldCommands()
        {
            try
            {
                var commandFiles = Directory.GetFiles("CommandsFolder", "*.dll");
                foreach (var file in commandFiles)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void CreateButton(Autodesk.Revit.UI.RibbonPanel panel, string scriptName, string scriptPath, int number)
        {
            try
            {
                var buttonData = new PushButtonData(
                $"Btn_{scriptName.Replace(" ", "_")}",
                scriptName,
                Assembly.GetExecutingAssembly().Location,
                $"TestLoadFamilyHandler.DynamoCommand1");

                // Сохраняем путь к скрипту в параметрах кнопки
                buttonData.ToolTip = $"{scriptName}";
                buttonData.LongDescription = scriptPath; // Используем для хранения пути
                MessageBox.Show(buttonData.LongDescription);
                // Добавление иконки если есть
                string iconPath = System.IO.Path.Combine("ScriptsFolder", $"{scriptName}.png");
                if (File.Exists(iconPath))
                {
                    buttonData.LargeImage = new BitmapImage(new Uri(iconPath));
                }

                panel.AddItem(buttonData);

                //CommandsDictionary[scriptName] = new Dictionary<string, string>()
                //{
                //    { scriptPath, $"DynamoCommand1" }
                //};
            }
            catch (Exception ex)
            {
                Autodesk.Revit.UI.TaskDialog.Show("Error", $"Failed to create button for {scriptName}: {ex.Message}");
            }
        }

        private void OnApplicationInitialized(object sender, ApplicationInitializedEventArgs e)
        {
            bool isExist = File.Exists($"{Assembly.GetExecutingAssembly().Location}\\ANDBIM.Revit.DS.Configuration.json");

            if (!isExist)
            {
                File.Create($"{Assembly.GetExecutingAssembly().Location}\\ANDBIM.Revit.DS.Configuration.json");
            }
            else
            {
                var configList = JsonConvert.DeserializeObject<List<ViewTab>>(File.ReadAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json"));

                foreach (ViewTab viewTab in configList)
                {
                    ThisApplication.CreateRibbonTab(viewTab.Name);

                    foreach (ViewPanel panel in viewTab.Panels)
                    {
                        var ribbonPanel = ThisApplication.CreateRibbonPanel(panel.Name);

                        foreach (ViewButtonGroup viewButtonGroup in panel.ButtonGroups)
                        {
                            if (viewButtonGroup.ButtonGroupType == ViewButtonGroupTypeEnum.StandartButton)
                            {
                                PushButtonData buttonData = new PushButtonData($"{viewButtonGroup.Buttons[0].Name}",
                                                                $"{viewButtonGroup.Buttons[0].Name}",
                                                                Assembly.GetExecutingAssembly().Location,
                                                                "ANDBIM.Revit.DS.DynamoStartCommand");

                                PushButton newButton = ribbonPanel.AddItem(buttonData) as PushButton;

                                newButton.ToolTip = viewButtonGroup.Buttons[0].Description;
                                newButton.LongDescription = viewButtonGroup.Buttons[0].LongDescription;
                            }

                            else
                            {
                                PushButtonData pushButtonData1 = new PushButtonData($"{viewButtonGroup.Buttons[0].Name}",
                                                                $"{viewButtonGroup.Buttons[0].Name}",
                                                                Assembly.GetExecutingAssembly().Location,
                                                                "ANDBIM.Revit.DS.DynamoStartCommand");
                                PushButtonData pushButtonData2 = new PushButtonData($"{viewButtonGroup.Buttons[1].Name}",
                                                                $"{viewButtonGroup.Buttons[1].Name}",
                                                                Assembly.GetExecutingAssembly().Location,
                                                                "ANDBIM.Revit.DS.DynamoStartCommand");
                                PushButtonData pushButtonData3 = new PushButtonData($"{viewButtonGroup.Buttons[2].Name}",
                                                                $"{viewButtonGroup.Buttons[2].Name}",
                                                                Assembly.GetExecutingAssembly().Location,
                                                                "ANDBIM.Revit.DS.DynamoStartCommand");

                                ribbonPanel.AddStackedItems(pushButtonData1, pushButtonData2, pushButtonData3);
                            }
                        }
                    }
                }
            }
        }
    }
}
