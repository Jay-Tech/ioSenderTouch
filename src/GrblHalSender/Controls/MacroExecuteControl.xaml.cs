
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GrblHalSender.GrblCore;
using GrblHalSender.GrblCore.Config;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for MacroExecuteControl.xaml
    /// </summary>
    public partial class MacroExecuteControl : UserControl
    {

        public MacroExecuteControl()
        {
            InitializeComponent();
            DataContextChanged += View_DataContextChanged;
        }
        public string MenuLabel { get { return (string)FindResource("MenuLabel"); } }

        private void macroExecuteControl_Loaded(object sender, RoutedEventArgs e)
        {
            Macros = GHalSenderConfig.Settings.Macros;
        }

        private void View_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null && e.OldValue is INotifyPropertyChanged)
                ((INotifyPropertyChanged)e.OldValue).PropertyChanged -= OnDataContextPropertyChanged;
            if (e.NewValue != null && e.NewValue is INotifyPropertyChanged)
                (e.NewValue as GrblViewModel).PropertyChanged += OnDataContextPropertyChanged;
        }

        private void OnDataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is GrblViewModel && Visibility == Visibility.Visible) switch (e.PropertyName)
            {
                case nameof(GrblViewModel.StreamingState):
                    if ((sender as GrblViewModel).IsJobRunning)
                        Visibility = Visibility.Hidden;
                    break;
            }
        }

        public static readonly DependencyProperty MacrosProperty = DependencyProperty.Register(nameof(MacroExecuteControl.Macros), typeof(ObservableCollection<Macro>), typeof(MacroExecuteControl), new PropertyMetadata(new PropertyChangedCallback(OnMacrosChanged)));
        public ObservableCollection<Macro> Macros
        {
            get { return (ObservableCollection<Macro>)GetValue(MacrosProperty); }
            set { SetValue(MacrosProperty, value); }
        }

        private static void OnMacrosChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as MacroExecuteControl).OnMacrosChanged();
        }
        private void OnMacrosChanged()
        {
            Macros.CollectionChanged += Macros_CollectionChanged;
            Macros_CollectionChanged(Macros, null);
        }
        private void Macros_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            IsMessageVisible = (sender as ObservableCollection<Macro>).Count == 0 ? Visibility.Visible : Visibility.Hidden;
        }

        public static readonly DependencyProperty IsMessageVisibleProperty = DependencyProperty.Register(nameof(IsMessageVisible), typeof(Visibility), typeof(MacroExecuteControl), new PropertyMetadata(Visibility.Visible));
        public Visibility IsMessageVisible
        {
            get { return (Visibility)GetValue(IsMessageVisibleProperty); }
            set { SetValue(IsMessageVisibleProperty, value); }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Macro macro = Macros.FirstOrDefault(o => o.Id == (int)(sender as Button).Tag);
            if (macro != null && (!macro.ConfirmOnExecute || MessageBox.Show(string.Format((string)FindResource("RunMacro"), macro.Name), "ioSender", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes))
                (DataContext as GrblViewModel).ExecuteMacro(macro.Code);
        }

        private void btn_Close(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Hidden;
        }

        private void button_Edit(object sender, RoutedEventArgs e)
        {
            //MacroEditor editor = new MacroEditor(Macros) {Owner = Application.Current.MainWindow};
            //editor.ShowDialog();
            //GHalSenderConfig.Settings.Save();
        }
    }
}
