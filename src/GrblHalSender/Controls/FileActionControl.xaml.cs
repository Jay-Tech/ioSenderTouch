
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GrblHalSender.GrblCore.Config;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for FileControl.xaml
    /// </summary>
    /// 

    public partial class FileActionControl : UserControl
    {
        private bool fileChanged = false;

        public FileActionControl()
        {
            InitializeComponent();
        }

        void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            GCode.File.Open();
        }
        void btnReload_Click(object sender, RoutedEventArgs e)
        {
            GCode.File.Load((DataContext as GrblViewModel).FileName);
        }

        void btnClose_Click(object sender, RoutedEventArgs e)
        {
            GCode.File.Close();
        }

        void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            editFile();
        }

        private async void editFile()
        {
            string fileName = (DataContext as GrblViewModel).FileName;

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    //FileName = @"D:\Notepad++\notepad++.exe",
                    //Arguments = '"' + fileName + '"'+ " -multiInst"
                    FileName = GHalSenderConfig.Settings.Base.Editor,
                    Arguments = '"' + fileName + '"'
                }
            };

            if (process.Start())
            {
                fileChanged = false;

                using (var watch = new FileSystemWatcher()
                {
                    Path = Path.GetDirectoryName(fileName),
                    Filter = Path.GetFileName(fileName),
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true
                })
                {
                    watch.Changed += File_Changed;

                    (DataContext as GrblViewModel).IsJobRunning = true;

                    await process.WaitForExitAsync();

                    (DataContext as GrblViewModel).IsJobRunning = false;

                    if (fileChanged)
                        GCode.File.Load(fileName);
                }
            }
        }

        private void File_Changed(object sender, FileSystemEventArgs e)
        {
            fileChanged = true;
        }
    }
}
