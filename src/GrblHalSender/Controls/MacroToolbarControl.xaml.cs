
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using GrblHalSender.GrblCore;
using GrblHalSender.GrblCore.Config;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for MacroToolbarControl.xaml
    /// </summary>
    public partial class MacroToolbarControl : UserControl
    {
        public MacroToolbarControl()
        {
            InitializeComponent();
        }

        private void macroToolbarControl_Loaded(object sender, RoutedEventArgs e)
        {
            Macros = GHalSenderConfig.Settings.Macros;
        }

        public static readonly DependencyProperty MacrosProperty = DependencyProperty.Register(nameof(MacroToolbarControl.Macros), typeof(ObservableCollection<Macro>), typeof(MacroToolbarControl));
        public ObservableCollection<Macro> Macros
        {
            get => (ObservableCollection<Macro>)GetValue(MacrosProperty);
            set => SetValue(MacrosProperty, value);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var macro = Macros.FirstOrDefault(o =>
            {
                var tag = (sender as Button)?.Tag;
                return tag != null && o.Id == (int)tag;
            });
            if (macro != null && (!macro.ConfirmOnExecute || MessageBox.Show(string.Format((string)FindResource("RunMacro"), macro.Name), "ioSender",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes))
                (DataContext as GrblViewModel)?.ExecuteMacro(macro.Code);
        }
    }
}
