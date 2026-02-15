

using System.Windows.Controls;
using GrblHalSender.ViewModels;
using GrblHalSender.Views;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for OffsetView.xaml
    /// </summary>
    public partial class OffsetView : UserControl
    {
        public OffsetView(GrblViewModel grblViewModel, ContentManager contentManager)
        {
            InitializeComponent();
            var vModel = new OffsetViewModel(grblViewModel);
            contentManager.RegisterViewAndModel("offsetView", vModel);
            DataContext = vModel;
            
        }
    
    }
}
