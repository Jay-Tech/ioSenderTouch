
using System.Windows.Controls;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls.Config
{
    /// <summary>
    /// Interaction logic for JogUiConfigControl.xaml
    /// </summary>
    public partial class JogUiConfigControl : UserControl
    {
        private readonly UIJoggingViewModel _uiJoggingModel;

        public JogUiConfigControl(GrblViewModel model)
        {
            InitializeComponent();
            
            _uiJoggingModel = new UIJoggingViewModel(model);
            DataContext = _uiJoggingModel;
        }
    }
}
