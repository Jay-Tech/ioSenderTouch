using System.Windows.Controls;
using GrblHalSender.GrblCore;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for SDCardView.xaml
    /// </summary>
    public partial class SDCardView : UserControl
    {
        private readonly ContentManager _contentManager;
        public SDCardView(GrblViewModel grblViewModel, ContentManager contentManager)
        {
            _contentManager = contentManager;
            InitializeComponent();
            var vm = new SdCardViewModel(grblViewModel);
            DataContext = vm;
            _contentManager.RegisterViewAndModel(nameof(SDCardView), vm);
        }
    }
}
