using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace ANDBIM.Revit.DS.Installer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class ResultWindow : Window
    {
        MainWindow MWindow;
        public ResultWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            MWindow = mainWindow;
            //Thread.Sleep(7000);
        }

        private void ResultWindow_Activated(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OpenTelegram_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("chrome", "https://t.me/andreevbimchanel");
            }

            catch
            {
                Process.Start("https://t.me/andreevbimchanel");
            }
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            MWindow.Close();
            Close();
        }
    }
}
