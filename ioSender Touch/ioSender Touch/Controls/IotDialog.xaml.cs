using System.Windows;

namespace ioSenderTouch.Controls
{
    /// <summary>
    /// Interaction logic for IotDialog.xaml
    /// </summary>
    public partial class IotDialog : Window
    {
        public IotDialog()
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
