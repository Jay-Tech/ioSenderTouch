
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GrblHalSender.GrblCore.Config;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls.Render
{
    /// <summary>
    /// Interaction logic for TextOverlayControl.xaml
    /// </summary>
    public partial class TextOverlayControl : UserControl
    {
        GrblViewModel model = null;
        
        public TextOverlayControl()
        {
            InitializeComponent();
            
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
             Foreground =  GHalSenderConfig.Settings.GCodeViewer.BlackBackground? Brushes.White : Brushes.Black;
        }

        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is GrblViewModel)
                model = DataContext as GrblViewModel;
            DataContext = (bool)e.NewValue ? model : null;
        }
    }
}
