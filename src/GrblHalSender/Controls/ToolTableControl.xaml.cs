using System.Windows.Controls;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for ToolTableControl.xaml
    /// </summary>
    public partial class ToolTableControl : UserControl
    {
        public ToolTableControl(GrblViewModel viewModel)
        {
            InitializeComponent();
            var viewModel1 = new ToolTableViewModel(viewModel);
            DataContext = viewModel1;

        }
    }
}
