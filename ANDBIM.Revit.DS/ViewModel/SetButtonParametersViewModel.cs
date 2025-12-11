using ANDBIM.Revit.DS.Model;
using ANDBIM.Revit.DS.Services;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace ANDBIM.Revit.DS.ViewModel
{
    public class SetButtonParametersViewModel : INotifyPropertyChanged
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

        public SetButtonParametersViewModel()
        {
        }

        public SetButtonParametersViewModel(ExternalCommandData commandData)
        {
            CommandData = commandData;

            RibbonControl ribbon = Autodesk.Windows.ComponentManager.Ribbon;

            // Iterate through the tabs to find the active one
            foreach (RibbonTab tab in ribbon.Tabs)
            {
                if (tab.IsActive)
                {
                    foreach (Autodesk.Windows.RibbonPanel ribbonPanel in tab.Panels)
                    {
                        if (ribbonPanel.Source.Name != "ANDBIM.DS.Settings")
                        {
                            Panels.Add(ribbonPanel.Source.Name);
                        }
                    }
                }
            }

            SelectedPanel = Panels.FirstOrDefault();

            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var arrayStr = thisAssemblyPath.Split('\\').ToList();
            arrayStr.Remove(arrayStr[arrayStr.Count - 1]);
            string folderPath = string.Join("\\", arrayStr.ToArray());
            AssemblyPath = folderPath;
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

        private string _buttonName { get; set; } = "";
        public string ButtonName
        {
            get => _buttonName;
            set
            {
                _buttonName = value; ;
                OnPropertyChanged("ButtonName");
            }
        }

        private string _imagePath { get; set; } = "";
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value; ;
                OnPropertyChanged("ImagePath");
            }
        }

        private BitmapImage _image { get; set; }
        public BitmapImage Image
        {
            get => _image;
            set
            {
                _image = value; ;
                OnPropertyChanged("Image");
            }
        }

        private BitmapImage _largeImage { get; set; }
        public BitmapImage LargeImage
        {
            get => _largeImage;
            set
            {
                _largeImage = value; ;
                OnPropertyChanged("LargeImage");
            }
        }

        private List<string> _panels { get; set; } = new List<string>();
        public List<string> Panels
        {
            get => _panels;
            set
            {
                _panels = value; ;
                OnPropertyChanged("Panels");
            }
        }

        private string _selectedPanel { get; set; }
        public string SelectedPanel
        {
            get => _selectedPanel;
            set
            {
                _selectedPanel = value; ;
                OnPropertyChanged("SelectedPanel");
            }
        }

        private string _scriptPath { get; set; } = "";
        public string ScriptPath
        {
            get => _scriptPath;
            set
            {
                _scriptPath = value; ;
                OnPropertyChanged("ScriptPath");
            }
        }

        private string _description { get; set; } = "";
        public string Description
        {
            get => _description;
            set
            {
                _description = value; ;
                OnPropertyChanged("Description");
            }
        }

        private string _longDescription { get; set; } = "";
        public string LongDescription
        {
            get => _longDescription;
            set
            {
                _longDescription = value; ;
                OnPropertyChanged("LongDescription");
            }
        }

        

        private RelayCommand _startCommand;

        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand ?? new RelayCommand(obj =>
                {
                    Autodesk.Revit.UI.RibbonPanel panel = GetPanelByName(SelectedPanel);

                    PushButtonData buttonDataAddTab = new PushButtonData($"{ButtonName}",
                                                                        $"{ButtonName}",
                                                                        Assembly.GetExecutingAssembly().Location,
                                                                        "ANDBIM.Revit.DS.DynamoStartCommand");

                    PushButton newButton = panel.AddItem(buttonDataAddTab) as PushButton;
                    newButton.ToolTip = $"{Description}";
                    newButton.LongDescription = $"{LongDescription}";
                    newButton.Image = Image;
                    newButton.LargeImage = LargeImage;

                    var configList = JsonConvert.DeserializeObject<List<ViewTab>>(File.ReadAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json"));

                    ViewPanel panelfromConfig = null;

                    var tab = (from ViewTab t in configList
                               where t.Name == GetActiveTabName()
                               select t).FirstOrDefault();

                    var viewPanel = (from ViewPanel v in tab.Panels
                                     where v.Name == panel.Name
                                     select v).FirstOrDefault();

                    if (tab != null)
                    {
                        foreach (ViewTab viewTab in configList)
                        {
                            if (viewTab.Name == GetActiveTabName())
                            {
                                if (viewPanel == null)
                                {
                                    viewPanel = new ViewPanel(panel.Name);
                                    viewTab.Panels.Add(viewPanel);
                                }

                                foreach (ViewPanel pan in tab.Panels)
                                {
                                    if (pan.Name == panel.Name)
                                    {
                                        pan.Name = panel.Name;
                                        pan.ButtonGroups.Add(new ViewButtonGroup(ViewButtonGroupTypeEnum.StandartButton, new ObservableCollection<ViewButton>()
                                        {
                                            new ViewButton(ImagePath, ScriptPath, Description, LongDescription, ButtonName)

                                        }));
                                    }
                                }

                                break;
                            }
                        }
                    }

                    File.WriteAllText($"{AssemblyPath}\\ANDBIM.Revit.DS.Configuration.json", JsonConvert.SerializeObject(configList));

                    RaiseCloseRequest();
                });
            }
        }

        private RelayCommand _getImageCommand;

        public RelayCommand GetImageCommand
        {
            get
            {
                return _getImageCommand ?? new RelayCommand(obj =>
                {
                    var openFileDialog = new OpenFileDialog();
                    var result = openFileDialog.ShowDialog();

                    if (result == true && openFileDialog.FileName != null 
                    && (openFileDialog.FileName.ToLower().Contains(".jpeg") || openFileDialog.FileName.ToLower().Contains(".png")))
                    {
                        ImagePath = openFileDialog.FileName;

                        try
                        {
                            //BitmapImage imgOriginal = new BitmapImage(new Uri(ImagePath));

                            Image = ImageWorker.ResizeBitmapImage(16, 16, ImagePath);
                            Thread.Sleep(1000);
                            LargeImage = ImageWorker.ResizeBitmapImage(32, 32, ImagePath);
                        }
                        catch (Exception ex)
                        {
                            ANDBIMErrorMessageWindow aNDBIMErrorMessageWindow = new ANDBIMErrorMessageWindow($"Не удалось загрузить изображение:\n{ex.Message}\n{ex.StackTrace}");
                            
                            aNDBIMErrorMessageWindow.ShowDialog();
                        }
                    }
                });
            }
        }

        private RelayCommand _getScriptCommand;

        public RelayCommand GetScriptCommand
        {
            get
            {
                return _getScriptCommand ?? new RelayCommand(obj =>
                {
                    var openFileDialog = new OpenFileDialog();
                    var result = openFileDialog.ShowDialog();

                    if (result == true && openFileDialog.FileName != null
                    && openFileDialog.FileName.Contains(".dyn"))
                    {
                        ScriptPath = openFileDialog.FileName;
                    }
                    else
                    {
                        ANDBIMErrorMessageWindow aNDBIMErrorMessageWindow = new ANDBIMErrorMessageWindow($"Не удалось загрузить скрипт!\nПопробуйте другой файл.");
                        
                        aNDBIMErrorMessageWindow.ShowDialog();
                    }
                });
            }
        }

        //public BitmapImage ResizeBitmapImage(BitmapImage original, int newWidth, int newHeight)
        //{
        //    var scaledBitmap = new TransformedBitmap();
        //    scaledBitmap.BeginInit();
        //    scaledBitmap.Source = original;

        //    // Создаем ScaleTransform
        //    ScaleTransform scale = new ScaleTransform(
        //        (double)newWidth / original.PixelWidth,
        //        (double)newHeight / original.PixelHeight,
        //        0, 0);

        //    scaledBitmap.Transform = scale;
        //    scaledBitmap.EndInit();

        //    // Конвертируем обратно в BitmapImage
        //    BitmapImage result = new BitmapImage();
        //    result.BeginInit();
        //    result.StreamSource = BitmapSourceToStream(scaledBitmap, ImagePath);
        //    result.EndInit();

        //    return result;
        //}

        //private MemoryStream BitmapSourceToStream(BitmapSource bitmapSource, string pathToImage)
        //{
        //    PngBitmapEncoder encoderPNG = new PngBitmapEncoder();
        //    JpegBitmapEncoder encoderJPEG = new JpegBitmapEncoder();
        //    var stream = new MemoryStream();

        //    if (ImagePath.ToLower().Contains(".jpeg"))
        //    {
        //        encoderJPEG.Frames.Add(BitmapFrame.Create(bitmapSource));

        //        encoderJPEG.Save(stream);
        //        stream.Position = 0;
        //    }
        //    else if (ImagePath.ToLower().Contains(".png"))
        //    {
        //        encoderPNG.Frames.Add(BitmapFrame.Create(bitmapSource));

        //        encoderPNG.Save(stream);
        //        stream.Position = 0;
        //    }
            
        //    return stream;
        //}

        private Autodesk.Revit.UI.RibbonPanel GetPanelByName(string name)
        {
            string tabName = GetActiveTabName();
            // Get all custom panels on the specified tab
            IList<Autodesk.Revit.UI.RibbonPanel> panels = CommandData.Application.GetRibbonPanels(tabName);

            // Find the panel by its name
            return panels.FirstOrDefault(p => p.Name == name);
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

                Autodesk.Revit.UI.RibbonPanel panel = CreatePanel(tabName, "Управление");

                panel.AddItem(buttonDataAddButton);
                panel.AddItem(buttonDataAddPanel);
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

        private Autodesk.Revit.UI.RibbonPanel CreatePanel(string tabName, string panelName)
        {
            try
            {
                return CommandData.Application.CreateRibbonPanel(tabName, panelName);
            }

            catch (Exception ex)
            {
                ANDBIMErrorMessageWindow aNDBIMErrorMessageWindow = new ANDBIMErrorMessageWindow($"Не удалось создать панель:\n{ex.Message}\n{ex.StackTrace}");
                RaiseCloseRequest();
                aNDBIMErrorMessageWindow.ShowDialog();
                //MessageBox.Show($"Не удалось создать панель:\n{ex.Message}\n{ex.StackTrace}", "Ошибка!");
                return null;
            }
        }
    }
}
