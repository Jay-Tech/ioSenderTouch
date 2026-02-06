using System.Windows.Controls;
using GrblHalSender.GrblCore.Config;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for AppUiSettings.xaml
    /// </summary>
    public partial class AppUiSettings : UserControl
    {
        private readonly AppUiSettingsConfig _uiConfig;

        public AppUiSettings()
        {
            InitializeComponent();
            
        }

       
    }
}
