
using System.Windows;
using System.Windows.Controls;
using GrblHalSender.ViewModels.Probing;

namespace GrblHalSender.Controls.Probing
{
    /// <summary>
    /// Interaction logic for CsSelectControl.xaml
    /// </summary>
    public partial class CsSelectControl : UserControl
    {
        public CsSelectControl()
        {
            InitializeComponent();
        }

        private void ClearG92_Click(object sender, RoutedEventArgs e)
        {
            var model = DataContext as ProbingViewModel;
            model.Grbl.ExecuteCommand("G92.1");
            if (!model.Grbl.IsParserStateLive)
                model.Grbl.ExecuteCommand("$G");
        }
    }
}
