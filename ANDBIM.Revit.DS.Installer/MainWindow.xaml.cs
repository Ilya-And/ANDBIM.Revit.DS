using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace ANDBIM.Revit.DS.Installer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Activated += MainWindow_Activated;
        }

        private void MainWindow_Activated(object sender, EventArgs e)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var arrayStr = thisAssemblyPath.Split('\\').ToList();
            arrayStr.Remove(arrayStr[arrayStr.Count - 1]);
            string folderPath = string.Join("\\", arrayStr.ToArray());

            //string[] filePathsToCopy2021 = Directory.GetFiles($"{folderPath}\\ANDBIM.Revit.DS\\R2021\\");
            //string[] filePathsToCopy2022 = Directory.GetFiles($"{folderPath}\\ANDBIM.Revit.DS\\R2022\\");
            //string[] filePathsToCopy2023 = Directory.GetFiles($"{folderPath}\\ANDBIM.Revit.DS\\R2023\\");
            //string[] filePathsToCopy2024 = Directory.GetFiles($"{folderPath}\\ANDBIM.Revit.DS\\R2024\\");
            //string[] filePathsToCopy2025 = Directory.GetFiles($"{folderPath}\\ANDBIM.Revit.DS\\R2025\\");
            //string[] filePathsToCopy2026 = Directory.GetFiles($"{folderPath}\\ANDBIM.Revit.DS\\R2026\\");
            //string[] filePathsToCopy2027 = Directory.GetFiles($"{folderPath}\\ANDBIM.Revit.DS\\R2027\\");

            InstallVersion("2021");
            InstallVersion("2022");
            InstallVersion("2023");
            InstallVersion("2024");
            InstallVersion("2025");
            InstallVersion("2026");
            InstallVersion("2027");

            StartDelayedAction();
        }

        private void StartDelayedAction()
        {
            // Создаем таймер
            DispatcherTimer timer = new DispatcherTimer();
            // Устанавливаем интервал (например, 7 сек)
            timer.Interval = TimeSpan.FromSeconds(7);
            // Подписываемся на событие Tick
            timer.Tick += Timer_Tick;
            // Запускаем таймер
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Останавливаем таймер, чтобы он сработал один раз
            (sender as DispatcherTimer).Stop();
            ResultWindow resultWindow = new ResultWindow(this);
            resultWindow.Show();

            //Close();
        }
        private void InstallVersion(string version)
        {
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            var arrayStr = thisAssemblyPath.Split('\\').ToList();
            arrayStr.Remove(arrayStr[arrayStr.Count - 1]);
            string folderPath = string.Join("\\", arrayStr.ToArray());

            bool isExist = Directory.Exists($"{folderPath}\\ANDBIM.Revit.DS\\R{version}\\");

            if (isExist)
            {
                List<string> filePathsToCopy = Directory.GetFiles($"{folderPath}\\ANDBIM.Revit.DS\\R{version}\\").ToList();

                bool isPathExist = Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ANDBIM\\ANDBIM.Revit.DS\\R{version}\\");
                //bool isPathExist = Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ANDBIM.Revit.DS\\R{version}");

                if (isPathExist) Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ANDBIM\\ANDBIM.Revit.DS\\R{version}\\", true);

                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ANDBIM\\ANDBIM.Revit.DS\\R{version}\\");

                foreach (string path in filePathsToCopy)
                {
                    string fileName = path.Split('\\')[path.Split('\\').ToList().Count - 1];

                    try
                    {
                        File.Copy(path, $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ANDBIM\\ANDBIM.Revit.DS\\R{version}\\{fileName}", true);
                    }

                    catch { }

                    //if (fileName.Contains(".addin"))
                    //{
                    //    try
                    //    {
                    //        File.Copy(path, $"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\Autodesk\\Revit\\Addins\\{version}\\{fileName}", true);
                    //    }
                    //    catch { }
                    //}

                    //else
                    //{
                    //    try
                    //    {
                    //        bool isPathExist = Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ANDBIM\\ANDBIM.Revit.DS\\R{version}\\");
                    //        //bool isPathExist = Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ANDBIM.Revit.DS\\R{version}");

                    //        if (isPathExist) Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ANDBIM\\ANDBIM.Revit.DS\\R{version}\\", true);

                    //        Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ANDBIM\\ANDBIM.Revit.DS\\R{version}\\");

                    //        File.Copy(path, $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\ANDBIM\\ANDBIM.Revit.DS\\R{version}\\{fileName}", true);
                    //    }

                    //    catch { }
                    //}
                }

                try
                {
                    string text = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
                        "<RevitAddIns>\r\n\t" +
                        "<AddIn Type=\"Application\">\r\n\t\t" +
                        "<Name>ANDBIM.Revit.DS</Name>\r\n\t\t" +
                        $"<Assembly>{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/ANDBIM/ANDBIM.Revit.DS/R{version}/ANDBIM.Revit.DS.dll</Assembly>\r\n\t\t" +
                        "<FullClassName>ANDBIM.Revit.DS.ANDBIMApplication</FullClassName>\r\n\t\t" +
                        "<ClientId>c6e2309e-31e8-420b-9f1f-99e1921993f8</ClientId>\r\n\t\t" +
                        "<VendorId>Andreev BIM</VendorId>\r\n\t\t" +
                        "<VendorDescription>Andreev BIM</VendorDescription>\r\n\t" +
                        "</AddIn>\r\n" +
                        "</RevitAddIns>";

                    File.WriteAllText($"{Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)}\\Autodesk\\Revit\\Addins\\{version}\\ANDBIM.Revit.DS.addin", text);
                }
                catch { }
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
