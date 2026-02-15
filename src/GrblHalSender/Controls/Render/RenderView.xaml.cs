

using System.Windows.Controls;
using System.Windows.Input;
using GrblHalSender.GrblCore;
using GrblHalSender.ViewModels;
using GrblHalSender.Views;

namespace GrblHalSender.Controls.Render
{
    public partial class RenderView : UserControl
    {
        private readonly GrblViewModel _grblViewModel;
        private static bool keyboardMappingsOk = false;
        private readonly RenderViewModel _model;

        public RenderView()
        {
            InitializeComponent();
        }
        public RenderView(GrblViewModel grblViewModel, ContentManager manager)
        {
            _grblViewModel = grblViewModel;
            DataContext = _grblViewModel;
            _grblViewModel.RenderVM = _model = new RenderViewModel(grblViewModel);
            InitializeComponent();
            manager.RegisterViewAndModel(nameof(RenderView), _model);
            grblViewModel.GrblInitialized += GrblViewModel_GrblInitialized;
            _model.PropertyChanged += _model_PropertyChanged;
        }

        private void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RenderViewModel.SdCardFileLoaded))
            {
                if (_model.SdCardFileLoaded)
                {
                    Open(GCode.File.Tokens);
                }
            }
        }

        private void GrblViewModel_GrblInitialized(object sender, System.EventArgs e)
        {
            GCode.File.Model = _grblViewModel;
            GCode.File.FileLoaded += File_FileLoaded;
        }

        private void File_FileLoaded(object sender, bool fileLoaded)
        {
            if (fileLoaded)
            {
                Open(GCode.File.Tokens);
            }
            else
            {
                Close();
            }
        }
        public Machine MachineView
        {
            get { return gcodeView.Machine; }
        }

        public void Close()
        {
            gcodeView.ClearViewport();
        }

        public void Open(List<GCodeToken> tokens)
        {
            gcodeView.Render(tokens);
        }

        private bool ToggleGrid(Key key)
        {
            MachineView.ShowGrid = !MachineView.ShowGrid;
            return true;
        }
        private bool ToggleJobEnvelope(Key key)
        {
            MachineView.ShowJobEnvelope = !MachineView.ShowJobEnvelope;
            return true;
        }
        private bool ToggleWorkEnvelope(Key key)
        {
            MachineView.ShowWorkEnvelope = !MachineView.ShowWorkEnvelope;
            return true;
        }
        private bool RestoreView(Key key)
        {
            gcodeView.RestoreView();
            return true;
        }
        private bool ResetView(Key key)
        {
            gcodeView.ResetView();
            return true;
        }

        private void ResetView_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            gcodeView.ResetView();
        }

        private void SaveView_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            gcodeView.SaveView();
        }

        private void RestoreView_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            gcodeView.RestoreView();
        }

        private void RenderControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!keyboardMappingsOk && DataContext is GrblViewModel)
            {
                KeypressHandler keyboard = _grblViewModel.Keyboard;

                keyboardMappingsOk = true;

                keyboard.AddHandler(Key.V, ModifierKeys.Control, ResetView);
                keyboard.AddHandler(Key.R, ModifierKeys.Control, RestoreView);
                keyboard.AddHandler(Key.G, ModifierKeys.Control, ToggleGrid);
                keyboard.AddHandler(Key.J, ModifierKeys.Control, ToggleJobEnvelope);
                keyboard.AddHandler(Key.W, ModifierKeys.Control, ToggleWorkEnvelope);
            }
        }

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_grblViewModel.GrblState.State == GrblStates.Tool)
                Comms.com.WriteByte(GrblConstants.CMD_CYCLE_START);
        }
    }
}
