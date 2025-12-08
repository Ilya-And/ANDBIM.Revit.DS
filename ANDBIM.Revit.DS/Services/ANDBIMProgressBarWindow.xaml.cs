using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;

namespace ANDBIM.Revit.DS.Services
{
    /// <summary>
    /// Логика взаимодействия для ANDBIMProgressBarWindow.xaml
    /// </summary>
    public partial class ANDBIMProgressBarWindow : Window
    {
        private delegate void ProgressBarDelegate();
        public ANDBIMProgressBarWindow(int count, string fileName)
        {
            InitializeComponent();
            Progress.IsIndeterminate = false;
            Progress.Minimum = 0;
            Progress.Maximum = count;

            FileName.Text = $"{fileName}";
        }
        public void Update()
        {
            Progress.Dispatcher.Invoke(new ProgressBarDelegate(UpdateProgress), DispatcherPriority.Background);
        }

        private void UpdateProgress()
        {
            Progress.Value += 1;
        }
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
