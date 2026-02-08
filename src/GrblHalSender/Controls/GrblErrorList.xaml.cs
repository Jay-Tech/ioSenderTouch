
using System.Windows;
using System.Windows.Controls;
using GrblHalSender.GrblCore;

namespace GrblHalSender.Controls
{
    public partial class GrblErrorList : UserControl
    {
        public GrblErrorList()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var list = (from entry in GrblErrors.List orderby entry.Key select new { entry.Key, entry.Value }).ToList();

            var ok = list.Where(x => x.Key == 0).FirstOrDefault();
            if (ok != null)
                list.Remove(ok);

            dgrErrors.ItemsSource = list;
        }
    }
}
