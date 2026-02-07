using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GrblHalSender.Controls;
using GrblHalSender.Controls.Probing;
using GrblHalSender.Controls.Render;
using GrblHalSender.GrblCore;
using GrblHalSender.GrblCore.Comands;
using GrblHalSender.GrblCore.Config;
using GrblHalSender.Utility;
using GrblHalSender.Views;

namespace GrblHalSender.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged, IDisposable
    {

        private GrblViewModel _model;
        private Controller _controller = null;
        private ToolsView _toolsView;
        private RenderView _renderView;
        private ProbingView _probeView;
        private SDCardView _sdView;
        private GrblConfigView _grblSettingView;
        private AppConfigView _grblAppSettings;
        private OffsetView _offsetView;
        private UtilityView _utilityView;
        private readonly ContentManager _contentManager;
        private object _view;
        private string _consoleModeText;
        private bool _showGCodeConsole;
        private readonly HandController _gamepad;
        public ICommand SwitchConsoleCommand { get; }
        public ICommand ChangeView { get; }
        public object View
        {
            get => _view;
            set
            {
                if (Equals(value, _view)) return;
                _view = value;
                OnPropertyChanged();
            }
        }
        public bool ShowGCodeConsole
        {
            get => _showGCodeConsole;
            set
            {
                if (value == _showGCodeConsole) return;
                _showGCodeConsole = value;
                OnPropertyChanged();
            }
        }
        public string ConsoleModeText
        {
            get => _consoleModeText;
            set
            {
                if (value == _consoleModeText) return;
                _consoleModeText = value;
                OnPropertyChanged();
            }
        }
        public HomeViewModel(GrblViewModel grblViewModel)
        {
            ChangeView = new Command(SetNewView);
            SwitchConsoleCommand = new Command(SwitchConsole);
            _model = grblViewModel;
            _contentManager = _model.ContentManager;
            _renderView = new RenderView(_model, _contentManager);
            _grblSettingView = new GrblConfigView(_model, _contentManager);
            _grblAppSettings = new AppConfigView(_model, _contentManager);
            _offsetView = new OffsetView(_model, _contentManager);
            _utilityView = new UtilityView(_model, _contentManager);
            GHalSenderConfig.Settings.OnConfigFileLoaded += AppConfiguationLoaded;
            _controller = new Controller(_model, GHalSenderConfig.Settings);
            _controller.SetupAndOpen(Application.Current.Dispatcher);
            InitSystem();
            BuildOptionalUi();
            GCode.File.FileLoaded += File_FileLoaded;
            _gamepad = new HandController(_model);
            ConsoleModeText = "Console";
            View = _renderView;
            _contentManager.SetActiveUiElement(nameof(RenderView));
        }
        private void AppConfiguationLoaded(object sender, EventArgs e)
        {
            _model.PollingInterval = GHalSenderConfig.Settings.Base.PollInterval;
            var controls = new ObservableCollection<UserControl>
            {
                new BasicConfigControl(),
                new ProbingConfigControl()
            };

            if (GHalSenderConfig.Settings.JogMetric.Mode != JogConfig.JogMode.Keypad)
            {
                controls.Add(new JogUiConfigControl(_model));
            }
            controls.Add(new AppUiSettings());
            if (GHalSenderConfig.Settings.JogMetric.Mode != JogConfig.JogMode.UI)
            {
                controls.Add(new JogConfigControl(_model));
            }
            controls.Add(new StripGCodeConfigControl());

            if (GHalSenderConfig.Settings.GCodeViewer.IsEnabled)
            {
                controls.Add(new RenderConfigControl());
            }
            _grblAppSettings.Setup(controls);
        }
        private void SwitchConsole(object obj)
        {
            ShowGCodeConsole = !ShowGCodeConsole;
            ConsoleModeText = ShowGCodeConsole ? "Console" : "GCode Viewer";
        }
        private void File_FileLoaded(object sender, bool fileLoaded)
        {
            ShowGCodeConsole = fileLoaded;
            ConsoleModeText = ShowGCodeConsole ? "Console" : "GCode Viewer";
        }
        private void BuildOptionalUi()
        {
            if (_model.HasSDCard)
            {
                _sdView = new SDCardView(_model, _contentManager);
            }
            if (_model.HasToolTable)
            {
                _toolsView = new ToolsView(_model,_contentManager);
            }
            if (GrblInfo.HasProbe && GrblSettings.ReportProbeCoordinates)
            {
                _model.HasProbing = true;
                _probeView = new ProbingView(_model, _contentManager);
            }
        }
        private bool InitSystem()
        {
            int timeout = 5;

            using (new UIUtils.WaitCursor())
            {
                _model.Poller.SetState(0);
                while (!GrblInfo.Get())
                {
                    if (--timeout == 0)
                    {
                        _model.Message = "Controller is not responding!";
                        return false;
                    }
                    Thread.Sleep(500);
                }
                GrblAlarms.Get();
                GrblErrors.Get();
                GrblSettings.Load();
                if (GrblInfo.IsGrblHAL)
                {
                    GrblParserState.Get();
                    GrblWorkParameters.Get();
                }
                else
                    GrblParserState.Get(true);
                _model.Poller.SetState(GHalSenderConfig.Settings.Base.PollInterval);
            }
            return true;
        }
        private void SetNewView(object x)
        {
            var newView = x.ToString();
            switch (newView)
            {
                case "offsetView":
                    View = _offsetView;
                    break;
                case nameof(SDCardView):
                    View = _sdView;
                    break;
                case "grblSettingsView":
                    View = _grblSettingView;
                    break;
                case "appSettingsView":
                    View  = _grblAppSettings;
                    break;
                case nameof(ProbingView):
                    View = _probeView;
                    break;
                case "utilityView":
                    View = _utilityView;
                    break;
                case "toolsView":
                    View = _toolsView;
                    break;
                case nameof(RenderView):
                    View = _renderView;
                    break;
                default:  View = _renderView;
                    break;
            }
            _contentManager.SetActiveUiElement(newView);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void Dispose()
        {
            _gamepad.Dispose();
        }
    }
}

