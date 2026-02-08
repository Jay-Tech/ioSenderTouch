
using System.Windows.Controls;
using System.Windows.Input;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls
{
    public partial class SpindleControl : UserControl
    {
        public SpindleControl()
        {
            InitializeComponent();
            cvRPM.PreviewKeyUp += txtPos_KeyPress;
        }

        private void txtPos_KeyPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !((GrblViewModel)DataContext).IsJobRunning)
            {
                ((GrblViewModel)DataContext)?.ExecuteCommand($"S{((NumericTextBox)sender).Value}");
            }
        }

    }
}