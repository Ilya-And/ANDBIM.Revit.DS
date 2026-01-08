using ANDBIM.Revit.DS.Services;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Dynamo.Search;
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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace ANDBIM.Revit.DS.ViewModel
{
    public class RemoveElementViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public ExternalCommandData CommandData;
        public string AssemblyPath { get; private set; }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
        public RemoveElementViewModel(ExternalCommandData commandData, ObservableCollection<ViewTab> viewTabs)
        {
            CommandData = commandData;
            TitleText = "Удаление вкладок";
            ViewElements = viewTabs;

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var arrayStr = thisAssemblyPath.Split('\\').ToList();
            arrayStr.Remove(arrayStr[arrayStr.Count - 1]);
            string folderPath = string.Join("\\", arrayStr.ToArray());
            AssemblyPath = folderPath;
        }

        private string _name{ get; set; } = "";
        public string Name
        {
            get => _name;
            set
            {
                _name = value; ;
                OnPropertyChanged("Name");
            }
        }

        private string _titleText { get; set; } = "Удаление элементов";
        public string TitleText
        {
            get => _titleText;
            set
            {
                _titleText = value; ;
                OnPropertyChanged("TitleText");
            }
        }
        private ObservableCollection<ViewTab> _viewElements { get; set; } = new ObservableCollection<ViewTab>();
        public ObservableCollection<ViewTab> ViewElements
        {
            get => _viewElements;
            set
            {
                _viewElements = value; ;
                OnPropertyChanged("ViewElements");
            }
        }

        private RelayCommand _startCommand;

        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand ?? new RelayCommand(obj =>
                {
                    var configList = JsonConvert.DeserializeObject<List<ViewTab>>(File.ReadAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json"));

                    List<ViewTab> newConfigList = configList;

                    RibbonControl ribbon = ComponentManager.Ribbon;

                    foreach (ViewTab viewTab in ViewElements)
                    {
                        if (viewTab.IsChecked)
                        {
                            foreach (RibbonTab ribTab in ribbon.Tabs)
                            {
                                if (ribTab.Name == viewTab.Name)
                                {
                                    ribbon.Tabs.Remove(ribTab);
                                    Dictionary<string, Dictionary<string, Autodesk.Revit.UI.RibbonPanel>> ribbonItems = GetRibbonItemsDictionary();

                                    try
                                    {
                                        ribbonItems.Remove(ribTab.Name);
                                    }
                                    catch { }

                                    newConfigList = configList.Where(t => t.Name != viewTab.Name).ToList();

                                    break;
                                }
                            }
                        }

                        else
                        {
                            foreach (ViewPanel panel in viewTab.Panels)
                            {
                                if (panel.IsChecked)
                                {
                                    foreach (RibbonTab ribTab in ribbon.Tabs)
                                    {
                                        var ribbonPanel = ribTab.Panels.Where(p => p.Source.Name == panel.Name).FirstOrDefault();

                                        if (ribbonPanel != null)
                                        {
                                            ribTab.Panels.Remove(ribbonPanel);
                                            Dictionary<string, Dictionary<string, Autodesk.Revit.UI.RibbonPanel>> ribbonItems = GetRibbonItemsDictionary();

                                            if (ribbonItems.TryGetValue(ribTab.Id, out var tabItem)) tabItem.Remove(panel.Name);

                                            var newConfigListToReplace = new List<ViewTab>();

                                            foreach (ViewTab vTab in newConfigList)
                                            {
                                                ViewPanel panelToRemove = null;

                                                foreach (ViewPanel pn in vTab.Panels)
                                                {
                                                    if (pn.Name == panel.Name) panelToRemove = pn;
                                                }

                                                if (panelToRemove != null) vTab.Panels.Remove(panelToRemove);

                                                newConfigListToReplace.Add(vTab);
                                            }

                                            newConfigList = newConfigListToReplace;
                                        }
                                    }
                                }

                                else
                                {
                                    foreach (ViewButtonGroup viewButtonGroup in panel.ButtonGroups)
                                    {
                                        foreach (ViewButton viewButton in viewButtonGroup.Buttons)
                                        {
                                            if (viewButton.IsChecked)
                                            {
                                                foreach (RibbonTab ribTab in ribbon.Tabs)
                                                {
                                                    var ribbonButton = ribTab.Panels.Where(p => p.Source.Name == panel.Name).Where(b => b.AutomationName == viewButton.Name).FirstOrDefault();

                                                    if (ribbonButton != null)
                                                    {
                                                        foreach (Autodesk.Windows.RibbonPanel ribbonPanel in ribTab.Panels)
                                                        {
                                                            if (ribbonPanel.AutomationName == panel.Name)
                                                            {
                                                                RibbonItemCollection items = ribbonPanel.Source.Items;

                                                                Autodesk.Windows.RibbonButton buttonToRemove = null;

                                                                foreach (Autodesk.Windows.RibbonItem item in items)
                                                                {
                                                                    if (item is Autodesk.Windows.RibbonButton)
                                                                    {
                                                                        if (((Autodesk.Windows.RibbonButton)item).AutomationName == viewButton.Name)
                                                                        {
                                                                            buttonToRemove = (Autodesk.Windows.RibbonButton)item;
                                                                            break;
                                                                        }
                                                                    }
                                                                }

                                                                if (buttonToRemove != null)
                                                                {
                                                                    ribbonPanel.Source.Items.Remove(buttonToRemove);
                                                                }
                                                            }
                                                        }

                                                        foreach (ViewTab vTab in newConfigList)
                                                        {
                                                            foreach (ViewPanel pan in vTab.Panels)
                                                            {
                                                                foreach (ViewButtonGroup vbg in pan.ButtonGroups)
                                                                {
                                                                    if (vbg.Buttons.Contains(viewButton))
                                                                    {
                                                                        vbg.Buttons.Remove(viewButton);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    File.WriteAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json", JsonConvert.SerializeObject(newConfigList));

                    RaiseCloseRequest();
                });
            }
        }

        private static Dictionary<string, Dictionary<string, Autodesk.Revit.UI.RibbonPanel>> GetRibbonItemsDictionary()
        {
            var uiApplicationType = typeof(UIApplication);
            var ribbonItemsProperty = uiApplicationType.GetProperty("RibbonItemDictionary",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
            var ribbonItems =
                (Dictionary<string, Dictionary<string, Autodesk.Revit.UI.RibbonPanel>>)ribbonItemsProperty
                    .GetValue(typeof(UIApplication));
            return ribbonItems;
        }

        private void CreateTab(string tabName)
        {
            try
            {
                CommandData.Application.CreateRibbonTab(tabName);

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

                Autodesk.Revit.UI.RibbonPanel panel = CreatePanel(tabName, "ANDBIM.DS.Settings");

                panel.AddItem(buttonDataAddButton);
                panel.AddItem(buttonDataAddPanel);

                var configList = JsonConvert.DeserializeObject<List<ViewTab>>(File.ReadAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json"));

                //ObservableCollection<ViewButtonGroup> viewButtonGroups = new ObservableCollection<ViewButtonGroup>()
                //{
                //    new ViewButtonGroup(ViewButtonGroupTypeEnum.StandartButton, new ObservableCollection<ViewButton>(){ new ViewButton("", "", "", "", "Создать кнопку")}),
                //    new ViewButtonGroup(ViewButtonGroupTypeEnum.StandartButton, new ObservableCollection<ViewButton>(){ new ViewButton("", "", "", "", "Создать панель")})

                //};

                //ObservableCollection<ViewPanel> viewPanels = new ObservableCollection<ViewPanel>()
                //{
                //    new ViewPanel("ANDBIM.DS.Settings", viewButtonGroups)
                //};

                ViewTab viewTabToSave = new ViewTab(tabName);
                if (configList == null)
                {
                    configList = new List<ViewTab>();
                }

                configList.Add(viewTabToSave);

                File.WriteAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json", JsonConvert.SerializeObject(configList));

                //PushButtonData buttonDataRemoveButton = new PushButtonData("Удалить кнопку",
                //                                                        "Удалить кнопку",
                //                                                        ANDBIMApplication.AssemblyPath,
                //                                                        "ANDBIM.Revit.DS.RibbonTabCreatorCommand");
                //PushButtonData buttonDataRemovePanel = new PushButtonData("Удалить панель",
                //                                                        "Удалить панель",
                //                                                        ANDBIMApplication.AssemblyPath,
                //                                                        "ANDBIM.Revit.DS.RibbonTabCreatorCommand");

            }

            catch (Exception ex)
            {
                ANDBIMErrorMessageWindow aNDBIMErrorMessageWindow = new ANDBIMErrorMessageWindow($"Не удалось создать вкладку:\n{ex.Message}\n{ex.StackTrace}");
                RaiseCloseRequest();
                aNDBIMErrorMessageWindow.ShowDialog();
                //MessageBox.Show($"Не удалось создать вкладку:\n{ex.Message}\n{ex.StackTrace}", "Ошибка!");
            }
        }

        private Autodesk.Revit.UI.RibbonPanel CreatePanel(string tabName, string panelName)
        {
            try
            {
                var ribbonPanel = CommandData.Application.CreateRibbonPanel(tabName, panelName);

                //var configList = JsonConvert.DeserializeObject<List<ViewTab>>(File.ReadAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json"));
                //var tab = (from ViewTab t in configList
                //           where t.Name == tabName
                //           select t).FirstOrDefault();

                //if (tab != null)
                //{
                //    foreach (ViewTab viewTab in configList)
                //    {
                //        if (viewTab.Name == tabName)
                //        {
                //            var viewPanel = new ViewPanel(panelName, new ObservableCollection<ViewButtonGroup>());

                //            viewTab.Panels.Add(viewPanel);
                //        }
                //    }
                //}

                //File.WriteAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json", JsonConvert.SerializeObject(configList));

                return ribbonPanel;
            }

            catch (Exception ex)
            {
                ANDBIMErrorMessageWindow aNDBIMErrorMessageWindow = new ANDBIMErrorMessageWindow($"Не удалось создать панель:\n{ex.Message}\n{ex.StackTrace}");
                aNDBIMErrorMessageWindow.ShowDialog();
                //MessageBox.Show($"Не удалось создать панель:\n{ex.Message}\n{ex.StackTrace}", "Ошибка!");
                return null;
            }
        }

        private string GetActiveTabName()
        {
            RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;

            string tabName = "";

            // Iterate through the tabs to find the active one
            foreach (RibbonTab tab in ribbon.Tabs)
            {
                if (tab.IsActive)
                {
                    tabName = tab.Name;
                }
            }

            return tabName;
        }
    }
}
