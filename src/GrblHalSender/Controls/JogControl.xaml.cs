

using GrblHalSender.GrblCore;
using GrblHalSender.GrblCore.Config;
using GrblHalSender.Utility;
using GrblHalSender.ViewModels;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for JogControl.xaml
    /// </summary>
    public partial class JogControl : UserControl, INotifyPropertyChanged
    {

        
        private string mode = "G21"; // Metric
        private bool softLimits = false;
        private int jogAxis = -1;
        private double limitSwitchesClearance = .5d, position = 0d;
        private KeypressHandler keyboard;
        private static bool keyboardMappingsOk = false;
        private GrblViewModel _grblViewModel;
        private double[] _distance = new double[4];
        private int[] _feedRate = new int[4];
        private DispatcherTimer _holdTimer;
        private string _holdButtonContent = string.Empty;
        private bool _contiousJogActive;


        private int _feedRate0;
        private int _feedRate1;
        private int _feedRate2;
        private int _feedRate3;

        private double _distance0;
        private double _distance1;
        private double _distance2;
        private double _distance3;

        private JogFeed _jogFeed;
        private JogStep _jogStep;
        private HomingPosition _homingPosition;
        public double Distance { get { return _distance[(int)JogStep]; } }
        public double FeedRate
        {
            get
            {
                return _feedRate[(int)JogFeed];
            }
        }
        public JogFeed JogFeed
        {
            get => _jogFeed;
            set
            {
                if (value == _jogFeed) return;
                _jogFeed = value;
                OnPropertyChanged();
            }
        }
        public JogStep JogStep
        {
            get => _jogStep;
            set
            {
                if (value == _jogStep) return;
                _jogStep = value;
                OnPropertyChanged();
            }
        }
        public int FeedRate3
        {
            get => _feedRate3;
            set
            {
                if (value == _feedRate3) return;
                _feedRate3 = value;
                OnPropertyChanged();
            }
        }
        public int FeedRate2
        {
            get => _feedRate2;
            set
            {
                if (value == _feedRate2) return;
                _feedRate2 = value;
                OnPropertyChanged();
            }
        }
        public int FeedRate1
        {
            get => _feedRate1;
            set
            {
                if (value == _feedRate1) return;
                _feedRate1 = value;
                OnPropertyChanged();
            }
        }
        public int FeedRate0
        {
            get => _feedRate0;
            set
            {
                if (value == _feedRate0) return;
                _feedRate0 = value;
                OnPropertyChanged();
            }
        }
        public double Distance0
        {
            get => _distance0;
            set
            {
                if (value == _distance0) return;
                _distance0 = value;
                OnPropertyChanged();
            }
        }
        public double Distance1
        {
            get => _distance1;
            set
            {
                if (value == _distance1) return;
                _distance1 = value;
                OnPropertyChanged();
            }
        }
        public double Distance2
        {
            get => _distance2;
            set
            {
                if (value == _distance2) return;
                _distance2 = value;
                OnPropertyChanged();
            }
        }
        public double Distance3
        {
            get => _distance3;
            set
            {
                if (value == _distance3) return;
                _distance3 = value;
                OnPropertyChanged();
            }
        }

        public JogControl()
        {
            InitializeComponent();
            _holdTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _holdTimer.Tick += HoldTimer_Tick;
            GHalSenderConfig.Settings.OnConfigFileLoaded += Settings_OnConfigFileLoaded;
        }
        private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _holdTimer.Start();
            if (sender is not Button button) return;
            _holdButtonContent = button.Content.ToString() ?? string.Empty;
            e.Handled = true;
        }

        private void HoldTimer_Tick(object? sender, EventArgs e)
        {
            _holdTimer.Stop();
            if (string.IsNullOrEmpty(_holdButtonContent)) return;
            _contiousJogActive = true;
            var distance = _holdButtonContent switch
            {
                "X+" => $"X{_grblViewModel?.MaxDistanceX.ToInvariantString()}",
                "Y+" => $"Y{_grblViewModel?.MaxDistanceY.ToInvariantString()}",
                "Z+" => $"Z{_grblViewModel?.MaxDistanceZ.ToInvariantString()}",
                "X-" => $"X-{_grblViewModel?.MaxDistanceX.ToInvariantString()}",
                "Y-" => $"Y-{_grblViewModel?.MaxDistanceY.ToInvariantString()}",
                "Z-" => $"Z-{_grblViewModel?.MaxDistanceZ.ToInvariantString()}",
                _ => string.Empty
            };
            var command = _holdButtonContent == "stop"
                ? $"{(char)GrblConstants.CMD_JOG_CANCEL}"
                : $"$J=G91{mode}{distance}F{Math.Ceiling(FeedRate).ToInvariantString()}";
            _grblViewModel?.ExecuteCommand(command);
        }

        private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            _holdTimer.Stop();
            
            if (_contiousJogActive)
            {
                _grblViewModel?.ExecuteCommand($"{(char)GrblConstants.CMD_JOG_CANCEL}");
            }
            else
            {
                JogCommand(_holdButtonContent);
                e.Handled = true;
            }
            _holdButtonContent = string.Empty;
            _contiousJogActive = false;

        }

        private void Settings_OnConfigFileLoaded(object? sender, EventArgs e)
        {
            _homingPosition = GHalSenderConfig.Settings.Base.HomePositionSetting;
            _grblViewModel = Grbl.GrblViewModel;
            _grblViewModel.GrblUnitChanged += GrblViewModelUnitChanged;
            JogStep = JogStep.Step3;
            JogFeed = JogFeed.Feed3;
            SetUpControl();
        }

        private void GrblViewModelUnitChanged(object? sender, Measurement e)
        {
            SetUpControl();
        }

        private void SetJogRate()
        {
            _grblViewModel.JogRate = FeedRate;
        }
        private void SetJogDistance()
        {
            _grblViewModel.JogStep = Distance;
        }

        private void SetUpControl()
        {
            mode = _grblViewModel.IsMetric ? "G21" : "G20";
            _feedRate = _grblViewModel.IsMetric ? GHalSenderConfig.Settings.JogUiMetric.Feedrate : GHalSenderConfig.Settings.JogUiImperial.Feedrate;
            _distance = _grblViewModel.IsMetric ? GHalSenderConfig.Settings.JogUiMetric.Distance : GHalSenderConfig.Settings.JogUiImperial.Distance;
            FeedRate3 = _feedRate[3];
            FeedRate2 = _feedRate[2];
            FeedRate1 = _feedRate[1];
            FeedRate0 = _feedRate[0];
            Distance3 = _distance[3];
            Distance2 = _distance[2];
            Distance1 = _distance[1];
            Distance0 = _distance[0];
            _grblViewModel.JogRate = FeedRate;
            _grblViewModel.JogStep = Distance;

        }

        private void distance_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (Enum.TryParse(button.Tag.ToString(), true, out JogStep step))
            {
                JogStep = step;
                SetJogDistance();
            }

        }
        private void feedRate_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) return;
            if (Enum.TryParse(button.Tag.ToString(), true, out JogFeed feed))
            {
                JogFeed = feed;
                SetJogRate();
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            JogCommand((string)(sender as Button)?.Tag == "stop" ? "stop" : (string)(sender as Button)?.Content);
        }

        private void JogCommand(string cmd)
        {

            if (cmd == "stop")
                cmd = ((char)GrblConstants.CMD_JOG_CANCEL).ToString();
            else
            {
                var t = _contiousJogActive;
                var jogDataDistance = cmd[1] == '-' ? -Distance : Distance;
                if (softLimits)
                {
                    var axis = GrblInfo.AxisLetterToIndex(cmd[0]);

                    if (jogAxis != -1 && axis != jogAxis)
                        return;

                    if (axis != jogAxis)
                    {
                        if (_grblViewModel != null)
                            position = jogDataDistance + _grblViewModel.MachinePosition.Values[axis];
                    }
                    else
                        position += jogDataDistance;

                    if (GrblInfo.ForceSetOrigin)
                    {
                        if (!GrblInfo.HomingDirection.HasFlag(GrblInfo.AxisIndexToFlag(axis)))
                        {
                            if (position > 0d)
                                position = 0d;
                            else if (position < (-GrblInfo.MaxTravel.Values[axis] + limitSwitchesClearance))
                                position = (-GrblInfo.MaxTravel.Values[axis] + limitSwitchesClearance);
                        }
                        else
                        {
                            if (position < 0d)
                                position = 0d;
                            else if (position > (GrblInfo.MaxTravel.Values[axis] - limitSwitchesClearance))
                                position = GrblInfo.MaxTravel.Values[axis] - limitSwitchesClearance;
                        }
                    }
                    else
                    {
                        if (position > -limitSwitchesClearance)
                            position = -limitSwitchesClearance;
                        else if (position < -(GrblInfo.MaxTravel.Values[axis] - limitSwitchesClearance))
                            position = -(GrblInfo.MaxTravel.Values[axis] - limitSwitchesClearance);
                    }

                    if (position == 0d)
                        return;

                    jogAxis = axis;

                    cmd =
                        $"$J=G53{mode}{cmd[..1]}{position.ToInvariantString()}F{Math.Ceiling(FeedRate).ToInvariantString()}";
                }
                else
                    cmd =
                        $"$J=G91{mode}{cmd[..1]}{jogDataDistance.ToInvariantString()}F{Math.Ceiling(FeedRate).ToInvariantString()}";
            }

            _grblViewModel?.ExecuteCommand(cmd);
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
    }


}
