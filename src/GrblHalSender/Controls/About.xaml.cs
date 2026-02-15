
using System.Windows;
using GrblHalSender.GrblCore;
using GrblHalSender.GrblCore.Config;

namespace GrblHalSender.Controls
{
    public partial class About : Window
    {
        string version;

        public About(string title)
        {
            InitializeComponent();

            Title = version = title;
        }

        private void About_Load(object sender, System.EventArgs e)
        {
            Title = version;
            if (GrblCore.Resources.IsLegacyController)
                Title += " (legacy mode)";

            GrblInfo.Get();
            txtGrblVersion.Content = GrblInfo.Version;
            txtGrblOptions.Content = GrblInfo.Options;
            txtGrblNewOpts.Content = GrblInfo.NewOptions;
            txtGrblConnection.Content = GHalSenderConfig.Settings.Base.PortParams;
            grpGrbl.Header = GrblInfo.Firmware;

            if (GrblInfo.Identity != "")
                grpGrbl.Header += ": " + GrblInfo.Identity;
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void clbButton_Click(object sender, System.EventArgs e)
        {
            GrblSettings.CopyToClipboard();
        }
    }
}

