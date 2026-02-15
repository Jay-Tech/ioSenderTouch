using System.Windows.Controls;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Views
{
    /// <summary>
    /// Interaction logic for Utility.xaml
    /// </summary>
    public partial class UtilityView : UserControl
    {
        private UtilityViewModel _model;
        public UtilityView(GrblViewModel grblViewModel, ContentManager contentManager)
        {

            _model = new UtilityViewModel(grblViewModel);
            DataContext = _model;
            contentManager.RegisterViewAndModel("utilityView", _model);
            InitializeComponent();
        }
    }
}
