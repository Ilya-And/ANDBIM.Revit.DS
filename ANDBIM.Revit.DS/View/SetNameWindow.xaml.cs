using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration.Assemblies;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// Логика взаимодействия для SetNameWindow.xaml
    /// </summary>
    public partial class SetNameWindow : Window
    {
        public ExternalCommandData CommandData;
        public bool IsPanel = false;
        private string TabName = "";
        public string AssemblyPath { get; private set; }
        public SetNameWindow(ExternalCommandData commandData, string tabName)
        {
            InitializeComponent();
            CommandData = commandData;
            IsPanel = true;
            TabName = tabName;

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var arrayStr = thisAssemblyPath.Split('\\').ToList();
            arrayStr.Remove(arrayStr[arrayStr.Count - 1]);
            string folderPath = string.Join("\\", arrayStr.ToArray());
            AssemblyPath = folderPath;
        }

        public SetNameWindow(ExternalCommandData commandData)
        {
            InitializeComponent();
            CommandData = commandData;

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var arrayStr = thisAssemblyPath.Split('\\').ToList();
            arrayStr.Remove(arrayStr[arrayStr.Count - 1]);
            string folderPath = string.Join("\\", arrayStr.ToArray());
            AssemblyPath = folderPath;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            if (IsPanel)
            {
                CreatePanel(TabName, nameTextBox.Text);

                
            }

            else
            {
                CreateTab(nameTextBox.Text);
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

                RibbonPanel panel = CreatePanel(tabName, "ANDBIM.DS.Settings");

                panel.AddItem(buttonDataAddButton);
                panel.AddItem(buttonDataAddPanel);

                var configList = JsonConvert.DeserializeObject<List<ViewTab>>(File.ReadAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json"));

                ObservableCollection<ViewButtonGroup> viewButtonGroups = new ObservableCollection<ViewButtonGroup>()
                {
                    new ViewButtonGroup(ViewButtonGroupTypeEnum.StandartButton, new ObservableCollection<ViewButton>(){ new ViewButton("", "", "", "", "Создать кнопку")}),
                    new ViewButtonGroup(ViewButtonGroupTypeEnum.StandartButton, new ObservableCollection<ViewButton>(){ new ViewButton("", "", "", "", "Создать панель")})

                };

                ObservableCollection<ViewPanel> viewPanels = new ObservableCollection<ViewPanel>()
                {
                    new ViewPanel("ANDBIM.DS.Settings", viewButtonGroups)
                };

                ViewTab viewTabToSave = new ViewTab(tabName, viewPanels);
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
                MessageBox.Show($"Не удалось создать вкладку:\n{ex.Message}\n{ex.StackTrace}", "Ошибка!");
            }
        }

        private RibbonPanel CreatePanel(string tabName, string panelName)
        {
            try
            {
                var ribbonPanel = CommandData.Application.CreateRibbonPanel(tabName, panelName);

                return ribbonPanel;
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось создать панель:\n{ex.Message}\n{ex.StackTrace}", "Ошибка!");
                return null;
            }
        }
    }
}
