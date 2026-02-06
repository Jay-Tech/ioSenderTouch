using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace GrblHalSender
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Application.Current.DispatcherUnhandledException += DispatcherOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();

            int p = 0, lng = 0;
            while (p < args.GetLength(0)) switch (args[p++])
                {
                    case "-locale":
                        if (p < args.GetLength(0))
                            lng = p;
                        break;
                }


            if (lng > 0)
            {
                Thread.CurrentThread.CurrentUICulture =
                 Thread.CurrentThread.CurrentCulture =
                  CultureInfo.DefaultThreadCurrentCulture =
                   CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(args[lng]); ;

                FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
            }

            base.OnStartup(e);
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            MessageBox.Show("Unhandled exception occured: " + (args.ExceptionObject as Exception).Message, "CurrentDomainException");
        }

        private void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            args.Handled = true;

            MessageBox.Show("Unhandled exception occured: " + (args.Exception as Exception).Message, "DispatcherException");
            Environment.Exit(-1);
        }

        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            args.SetObserved();

            MessageBox.Show("Unhandled exception occured: " + (args.Exception.GetBaseException() as Exception).Message, "TaskSchedulerException");
        }
    }

}
