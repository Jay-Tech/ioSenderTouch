
using System.Windows;

namespace GrblHalSender.Controls
{
    public partial class ConsoleWindow : Window
    {
        private bool userClosing = true;

        public ConsoleWindow()
        {
            InitializeComponent();
        }

        public new void Close()
        {
            userClosing = false;
            base.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (userClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
