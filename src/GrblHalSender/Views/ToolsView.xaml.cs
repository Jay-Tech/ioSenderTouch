
using System.Windows.Controls;
using GrblHalSender.GrblCore;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Views
{
    public partial class ToolsView : UserControl
    {
        public ToolsView(GrblViewModel model, ContentManager contentManager)
        {
            InitializeComponent();
            var vModel = new ToolsViewModel(model);
            contentManager.RegisterViewAndModel("toolsView", vModel);
            DataContext = vModel;
        }
        
    }
}
