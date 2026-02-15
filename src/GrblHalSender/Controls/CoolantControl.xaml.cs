
using System.Windows.Controls;
using GrblHalSender.GrblCore;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls
{
    public partial class CoolantControl : UserControl
    {

        public CoolantControl()
        {
            InitializeComponent();
        }

        private void chkBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var com = GrblCommand.Mist;
            switch ((string)(sender as Button)?.Tag)
            {
                case "Flood":
                    (DataContext as GrblViewModel)?.ExecuteCommand(GrblCommand.Flood);
                    break;
                case "Mist":
                    (DataContext as GrblViewModel)?.ExecuteCommand(GrblCommand.Mist);
                    break;
                default:
                    (DataContext as GrblViewModel)?.ExecuteCommand(GrblCommand.Fan);
                    break;
            }
        }
    }
}
