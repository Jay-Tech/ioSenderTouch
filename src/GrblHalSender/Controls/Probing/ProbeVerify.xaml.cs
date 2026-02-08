
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GrblHalSender.GrblCore;
using GrblHalSender.ViewModels;
using GrblHalSender.ViewModels.Probing;

namespace GrblHalSender.Controls.Probing
{

    public partial class ProbeVerify : Window
    {
        public ProbeVerify(ProbingViewModel model)
        {
            InitializeComponent();

            DataContext = model;
            model.Grbl.PropertyChanged += Grbl_PropertyChanged;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var parent = Application.Current.MainWindow;

            Left = parent.Left + (parent.Width - Width) / 2d;
            Top = parent.Top + (parent.Height - Height) / 2d;

            (sender as Window).Dispatcher.Invoke(new System.Action(() =>
            {
                (sender as Window).MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }), DispatcherPriority.ContextIdle);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (DataContext as ProbingViewModel).Grbl.PropertyChanged -= Grbl_PropertyChanged;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!e.Handled && e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None)
                Close();
        }

        private void Grbl_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GrblViewModel.Signals) && (sender as GrblViewModel).Signals.Value.HasFlag(Signals.Probe))
            {
                (DataContext as ProbingViewModel).ProbeVerified = true;
                Close();
            }
        }
    }
}
