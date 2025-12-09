using ANDBIM.Revit.DS.ViewModel;
using Autodesk.Revit.UI;
using Autodesk.Windows;
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
    public partial class SetButtonParametersWindow : Window
    {
        public ExternalCommandData CommandData;
        public bool IsPanel = false;
        private string TabName = "";

        public string AssemblyPath { get; private set; }
        public SetButtonParametersWindow(ExternalCommandData commandData)
        {
            InitializeComponent();
            CommandData = commandData;

            SetButtonParametersViewModel vm = new SetButtonParametersViewModel(commandData);
            vm.CloseRequest += (s, e) => Close();

            DataContext = vm;
        }
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        //private void ButtonStart_Click(object sender, RoutedEventArgs e)
        //{
        //    Autodesk.Revit.UI.RibbonPanel panel = GetPanelByName(panelNameTextBox.Text);

        //    PushButtonData buttonDataAddTab = new PushButtonData($"{nameTextBox.Text}",
        //                                                        $"{nameTextBox.Text}",
        //                                                        Assembly.GetExecutingAssembly().Location,
        //                                                        "ANDBIM.Revit.DS.DynamoStartCommand");

        //    PushButton newButton = panel.AddItem(buttonDataAddTab) as PushButton;
        //    newButton.ToolTip = $"{descriptionTextBox.Text}";
        //    newButton.LongDescription = $"{longDescriptionTextBox.Text}";

        //    var configList = JsonConvert.DeserializeObject<List<ViewTab>>(File.ReadAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json"));

        //    ViewPanel panelfromConfig = null;

        //    var tab = (from ViewTab t in configList
        //               where t.Name == GetActiveTabName()
        //               select t).FirstOrDefault();

        //    var viewPanel = (from ViewPanel v in tab.Panels
        //                    where v.Name == panel.Name
        //                    select v).FirstOrDefault();

        //    if (tab != null)
        //    {
        //        foreach (ViewTab viewTab in configList)
        //        {
        //            if (viewTab.Name == GetActiveTabName())
        //            {
        //                if (viewPanel == null)
        //                {
        //                    viewPanel = new ViewPanel(panel.Name);
        //                    viewTab.Panels.Add(viewPanel);
        //                }

        //                foreach (ViewPanel pan in tab.Panels)
        //                {
        //                    if (pan.Name == panel.Name)
        //                    {
        //                        pan.Name = panel.Name;
        //                        pan.ButtonGroups.Add(new ViewButtonGroup(ViewButtonGroupTypeEnum.StandartButton, new ObservableCollection<ViewButton>()
        //                        {
        //                            new ViewButton("", "", $"{descriptionTextBox.Text}", $"{longDescriptionTextBox.Text}", $"{nameTextBox.Text}")

        //                        }));
        //                    }
        //                }

        //                break;
        //            }
        //        }
        //    }

        //    File.WriteAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json", JsonConvert.SerializeObject(configList));
        //}

        //private Autodesk.Revit.UI.RibbonPanel GetPanelByName(string name)
        //{
        //    string tabName = GetActiveTabName();
        //    // Get all custom panels on the specified tab
        //    IList<Autodesk.Revit.UI.RibbonPanel> panels = CommandData.Application.GetRibbonPanels(tabName);

        //    // Find the panel by its name
        //    return panels.FirstOrDefault(p => p.Name == name);
        //}

        //private string GetActiveTabName()
        //{
        //    RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;

        //    string tabName = "";

        //    // Iterate through the tabs to find the active one
        //    foreach (RibbonTab tab in ribbon.Tabs)
        //    {
        //        if (tab.IsActive)
        //        {
        //            tabName = tab.Name;
        //        }
        //    }

        //    return tabName;
        //}

        //private void CreateTab(string tabName)
        //{
        //    try
        //    {
        //        CommandData.Application.CreateRibbonTab(tabName);

        //        PushButtonData buttonDataAddButton = new PushButtonData("Создать кнопку",
        //                                                                "Создать кнопку",
        //                                                                Assembly.GetExecutingAssembly().Location,
        //                                                                "ANDBIM.Revit.DS.RibbonButtonCreatorCommand");
        //        PushButtonData buttonDataAddPanel = new PushButtonData("Создать панель",
        //                                                                "Создать панель",
        //                                                                Assembly.GetExecutingAssembly().Location,
        //                                                                "ANDBIM.Revit.DS.RibbonPanelCreatorCommand");

        //        Autodesk.Revit.UI.RibbonPanel panel = CreatePanel(tabName, "Управление");

        //        panel.AddItem(buttonDataAddButton);
        //        panel.AddItem(buttonDataAddPanel);
        //        //PushButtonData buttonDataRemoveButton = new PushButtonData("Удалить кнопку",
        //        //                                                        "Удалить кнопку",
        //        //                                                        ANDBIMApplication.AssemblyPath,
        //        //                                                        "ANDBIM.Revit.DS.RibbonTabCreatorCommand");
        //        //PushButtonData buttonDataRemovePanel = new PushButtonData("Удалить панель",
        //        //                                                        "Удалить панель",
        //        //                                                        ANDBIMApplication.AssemblyPath,
        //        //                                                        "ANDBIM.Revit.DS.RibbonTabCreatorCommand");

        //    }

        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Не удалось создать вкладку:\n{ex.Message}\n{ex.StackTrace}", "Ошибка!");
        //    }
        //}

        //private Autodesk.Revit.UI.RibbonPanel CreatePanel(string tabName, string panelName)
        //{
        //    try
        //    {
        //        return CommandData.Application.CreateRibbonPanel(tabName, panelName);
        //    }

        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Не удалось создать панель:\n{ex.Message}\n{ex.StackTrace}", "Ошибка!");
        //        return null;
        //    }
        //}

        //private void StandartButton_Checked(object sender, RoutedEventArgs e)
        //{

        //}

        //private void GetPathToScript_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //private void GetPathToImage_Click(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
