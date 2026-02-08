

using System.Windows.Controls;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls.Config
{
    /// <summary>
    /// Interaction logic for JogSetupControl.xaml
    /// </summary>
    public partial class JogConfigControl : UserControl
    {
        public JogConfigControl(GrblViewModel grblViewModel)
        {
            InitializeComponent();
            DataContext = new JogConfigControlViewModel(grblViewModel);
        }

    }
    
}
