
using System.Windows.Controls;
using GrblHalSender.GrblCore;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls
{
    public partial class WorkParametersControl : UserControl
    {
        public WorkParametersControl()
        {
            InitializeComponent();
        }
        private void cbxOffset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1 && ((ComboBox)sender).IsDropDownOpen)
                (DataContext as GrblViewModel)?.ExecuteCommand(((CoordinateSystem)e.AddedItems[0]).Code);
        }
    }
}