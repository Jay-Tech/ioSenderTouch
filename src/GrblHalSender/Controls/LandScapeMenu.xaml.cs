using System.Windows;
using System.Windows.Controls;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for LandScapeMenu.xaml
    /// </summary>
    public partial class LandScapeMenu : UserControl
    {
        public LandScapeMenu()
        {
            InitializeComponent();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        
        private void MaximizeClick(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != null)
            {
                AdjustWindowSize();
            }
        }
        private void AdjustWindowSize()
        {
            if (App.Current.MainWindow.WindowState == WindowState.Maximized)
            {
                App.Current.MainWindow.WindowState = WindowState.Normal;
                MaximizeButton.Content = "";
            }
            else if (App.Current.MainWindow.WindowState == WindowState.Normal)
            {
                App.Current.MainWindow.WindowState = WindowState.Maximized;
                MaximizeButton.Content = "";
            }
        }
    }
}
