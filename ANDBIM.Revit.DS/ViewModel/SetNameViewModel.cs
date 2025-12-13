using ANDBIM.Revit.DS.Services;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ANDBIM.Revit.DS.ViewModel
{
    public class SetNameViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public ExternalCommandData CommandData;
        public bool IsPanel = false;
        private string TabName = "";
        public string AssemblyPath { get; private set; }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
        public SetNameViewModel(ExternalCommandData commandData, string tabName)
        {
            CommandData = commandData;
            IsPanel = true;
            TabName = tabName;
            TitleText = "Создание панели инструментов";

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var arrayStr = thisAssemblyPath.Split('\\').ToList();
            arrayStr.Remove(arrayStr[arrayStr.Count - 1]);
            string folderPath = string.Join("\\", arrayStr.ToArray());
            AssemblyPath = folderPath;
        }

        public SetNameViewModel(ExternalCommandData commandData)
        {
            CommandData = commandData;
            TitleText = "Создание вкладки";

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

        private string _titleText { get; set; } = "Создание вкладки";
        public string TitleText
        {
            get => _titleText;
            set
            {
                _titleText = value; ;
                OnPropertyChanged("TitleText");
            }
        }

        private RelayCommand _startCommand;

        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand ?? new RelayCommand(obj =>
                {
                    if (IsPanel)
                    {
                        CreatePanel(TabName, Name);

                        RaiseCloseRequest();
                    }

                    else
                    {
                        CreateTab(Name);

                        RaiseCloseRequest();
                    }
                });
            }
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
