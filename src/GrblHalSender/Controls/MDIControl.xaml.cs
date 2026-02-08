
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GrblHalSender.GrblCore;
using GrblHalSender.ViewModels;

namespace GrblHalSender.Controls
{
    public partial class MDIControl : UserControl
    {
        private ICommand SendCommand { get; }
        private GrblViewModel _model;
        public MDIControl()
        {
            InitializeComponent();
            Commands = new ObservableCollection<string>();
        }

        public new bool IsFocused { get { return TxtMdi.IsKeyboardFocusWithin; } }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(string), typeof(MDIControl), new PropertyMetadata(""));
        public string Command
        {
            get { return (string)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandsProperty = DependencyProperty.Register(nameof(Commands), typeof(ObservableCollection<string>), typeof(MDIControl));
        public ObservableCollection<string> Commands
        {
            get { return (ObservableCollection<string>)GetValue(CommandsProperty); }
            set { SetValue(CommandsProperty, value); }
        }

        private void txtMDI_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return && _model.MDICommand.CanExecute(null))
            {
                if (_model == null)
                {
                    _model = Grbl.GrblViewModel;
                }
                string cmd = (sender as ComboBox).Text;
                if (!string.IsNullOrEmpty(cmd) && (Commands.Count == 0 || Commands[0] != cmd))
                    Commands.Insert(0, cmd);
                if (_model.GrblError != 0)
                    _model.ExecuteCommand("");
                _model.MDICommand.Execute(cmd);
                (sender as ComboBox).SelectedIndex = -1;
            }
        }

        private void txtMDI_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cmbTextBox = (TextBox)(sender as ComboBox).Template.FindName("PART_EditableTextBox", (sender as ComboBox));
            if (cmbTextBox != null)
            {
                cmbTextBox.Focus();
                cmbTextBox.CaretIndex = cmbTextBox.Text.Length;
            }
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Command) && !Commands.Contains(Command))
            {
                Commands.Insert(0, Command);
            }
            _model.ExecuteCommand(Command.ToUpper().Trim());
            TxtMdi.SelectedIndex = -1;
        }

        private void MDIControl_Loaded(object sender, RoutedEventArgs e)
        {
            var mdi = TxtMdi.Template.FindName("PART_EditableTextBox", TxtMdi) as TextBox;
            _model = Grbl.GrblViewModel;
            DataContext = _model;
            if(mdi != null)
                mdi.Tag = "MDI";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            var item = button.Content.ToString();
            var txt = TxtMdi.Text;
            switch (item)
            {
                case "Enter":
                    ProcessEnterAndCommand(txt);
                    break;
                case "Delete":
                    if(string.IsNullOrEmpty(txt))return;
                    var remove = txt.Remove(txt.Length - 1, 1);
                    TxtMdi.Text = remove;
                    break;
                case "Space":
                    TxtMdi.Text = txt + " ";
                    break;
                default:
                    TxtMdi.Text = txt + item;
                    break;
            }
        }

        private void ProcessEnterAndCommand(string txt)
        {
            var command = TxtMdi.Text;
            if(string.IsNullOrEmpty(TxtMdi.Text))return;
            _model.ExecuteCommand(command.ToUpper().Trim());
            TxtMdi.Text = string.Empty;
            TxtMdi.SelectedIndex = -1;
        }

    }
}
