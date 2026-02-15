using System.Windows;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for GrblHALLSenderDialog.xaml
    /// </summary>
    public partial class GrblHALLSenderDialog : Window
    {
        public GrblHALLSenderDialog()
        {
            InitializeComponent();
        }
        public string ResponseText
        {
            get => ResponseTextBox.Text;
            set => ResponseTextBox.Text = value;
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
