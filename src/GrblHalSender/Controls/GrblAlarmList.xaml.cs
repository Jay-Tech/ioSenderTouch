
using System.Windows;
using System.Windows.Controls;
using GrblHalSender.GrblCore;

namespace GrblHalSender.Controls
{
    public partial class GrblAlarmList : UserControl
    {
        public GrblAlarmList()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dgrAlarms.ItemsSource = (from entry in GrblAlarms.List orderby entry.Key select new { entry.Key, entry.Value }).ToList();
        }
    }
}
