using GameInputDotNet;
using GameInputDotNet.Interop.Enums;
using GrblHalSender.GrblCore;
using GrblHalSender.GrblCore.Config;
using GrblHalSender.ViewModels;
using System.Diagnostics;
using System.Globalization;

namespace GrblHalSender.Utility
{
    public class HandController : IDisposable
    {
        private const string JogHeader = "$J = G91G21";

        private GrblViewModel _grblViewModel;
        private bool _singleActionPress;
        private GameInputGamepadButtons _previousDown;
        private int[] _feedRate;
        private double[] _distanceRate;
        private bool _stepMode;
        private bool _jogProcessed;
        private Task _buttonPollThread;
        private int _pollRate = 20;
        CancellationTokenSource _cancellationTokenSource;
        private bool _joystickJogging;
        private bool _continuousJogActive;
        private bool _fallOverCutureFix;


        public double DistanceRate => _distanceRate?[(int)JogStepRate] ?? 1;
        public double FeedRate => _feedRate?[(int)JogFeedRate] ?? 1000;
        public JogFeed JogFeedRate { get; set; }
        public JogStep JogStepRate { get; set; }

        public HandController(GrblViewModel grblViewModel)
        {
            _grblViewModel = grblViewModel;
            _grblViewModel.GrblInitialized += _grblViewModel_GrblInitialized;
        }
        ~HandController()
        {
            Dispose(false);
        }
        private void _grblViewModel_GrblInitialized(object sender, EventArgs e)
        {
            SetupRates();
        }

        private void SetupRates()
        {
            try
            {
                var isMetric = GrblSettings.GetInteger(GrblSetting.ReportInches) == 0;
                _distanceRate = isMetric
                    ? GHalSenderConfig.Settings.JogUiMetric.Distance
                    : GHalSenderConfig.Settings.JogUiImperial.Distance;
                _feedRate = isMetric
                    ? GHalSenderConfig.Settings.JogUiMetric.Feedrate
                    : GHalSenderConfig.Settings.JogUiImperial.Feedrate;
                JogStepRate = (JogStep)Array.FindIndex(_distanceRate, row => row.Equals(_grblViewModel.JogStep));
                JogFeedRate = (JogFeed)Array.FindIndex(_feedRate, row => row.Equals((int)_grblViewModel.JogRate));

                SetUpGamePad();
            }
            catch (Exception e)
            {
                //
            }
        }

        private void SetUpGamePad()
        {
            if (Thread.CurrentThread.CurrentCulture.Name != "en-US")
            {
                _fallOverCutureFix = true;
            }
            var cultureInfo = new CultureInfo("en-US");
            if (_buttonPollThread?.Status == TaskStatus.Running) return;
            _cancellationTokenSource = new CancellationTokenSource();

            _buttonPollThread = Task.Factory.StartNew(() => PollPad(_cancellationTokenSource, cultureInfo),
                TaskCreationOptions.LongRunning);
        }

        private void PollPad(CancellationTokenSource cancellationToken, CultureInfo cultureInfo)
        {
            using var _gameInput = GameInput.Create();

            // Set focus allow capture of inputs regardless of program focus.
            _gameInput.SetFocusPolicy(GameInputFocusPolicy.EnableBackgroundInput);
            while (true)
            {
                var mode = _grblViewModel.IsMetric ? "G21" : "G20";
                var useImperial = !_grblViewModel.IsMetric;
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                // Slow down the loop so we don't get spammed.
                Thread.Sleep(100);
                GameInputReading? reading = null;

                try
                {
                    // Get the current frame's "Keyboard" inputs.
                    reading = _gameInput.GetCurrentReading(GameInputKind.Gamepad);
                }
                catch (GameInputException)
                {
                    // Handle exceptions and unprepared reading from first frame.
                    continue;
                }

                if (reading is null) continue;

                using (reading)
                {
                    // Get the current state. 
                    var state = reading.GetGamepadState();

                    // Nothing pressed we can jump to the next loop.
                    if (state is null) continue;
                 

                    double step = 0;
                    var zCurrent = Math.Abs(_grblViewModel.MachinePosition.Z);
                    var xCurrent = _grblViewModel.MachinePosition.X;
                    var yCurrent = Math.Abs(_grblViewModel.MachinePosition.Y);
                    //_jogProcessed = false;
                    if (_stepMode && _previousDown != state.Buttons)
                    {
                        _jogProcessed = false;
                        Debug.WriteLine($"Job Processed State {_jogProcessed}");
                    }

                    string command;
                    switch (state.Buttons)
                    {
                        case GameInputGamepadButtons.None:
                            _singleActionPress = false;
                            _jogProcessed = false;
                            _continuousJogActive = false;
                            if (state.LeftTrigger == 0)
                            {
                                _stepMode = false;
                            }
                            break;

                        case GameInputGamepadButtons.B:
                            if (_continuousJogActive) continue;
                            _continuousJogActive = true;
                            var zMax = useImperial ? _grblViewModel.MaxDistanceZ / 25.4 : _grblViewModel.MaxDistanceX;
                            step = zMax - zCurrent;
                            command = FormattableString.Invariant($"$J = G91{mode}Z{step}F{_grblViewModel.JogRate}");
                            ProcessJogCommand(command);
                            break;
                        
                        case GameInputGamepadButtons.A:
                            if (_continuousJogActive) continue;
                            _continuousJogActive = true;
                            step = zCurrent;
                            command = FormattableString.Invariant($"$J = G91{mode}Z-{Math.Abs(step)}F{_grblViewModel.JogRate}");
                            ProcessJogCommand(command);
                            break;
                        
                        case GameInputGamepadButtons.DPadLeft:
                            if (_continuousJogActive) continue;
                            _continuousJogActive = true;
                            step = xCurrent;
                            command = FormattableString.Invariant($"$J = G91{mode}X-{Math.Abs(step)}F{_grblViewModel.JogRate}"); ;
                            ProcessJogCommand(command);
                            break;
                       
                        case GameInputGamepadButtons.DPadRight:
                            if (_continuousJogActive) continue;
                            _continuousJogActive = true;
                            var xMax = useImperial ? _grblViewModel.MaxDistanceX / 25.4 :
                                _grblViewModel.MaxDistanceX;
                            step = xMax - xCurrent;
                            command = FormattableString.Invariant($"$J = G91{mode}X{step}F{_grblViewModel.JogRate}");
                            ProcessJogCommand(command);
                            break;
                        
                        case GameInputGamepadButtons.DPadUp:
                            if (_continuousJogActive) continue;
                            _continuousJogActive = true;
                            //var yMax = useImperial ? _grblViewModel.MaxDistanceY / 25.4 :
                            //    _grblViewModel.MaxDistanceY;
                            //step = yMax - yCurrent;
                            step = yCurrent;
                            command = FormattableString.Invariant($"$J = G91{mode}Y-{step}F{_grblViewModel.JogRate}");
                            ProcessJogCommand(command);
                            break;
                       
                        case GameInputGamepadButtons.DPadDown:
                            if (_continuousJogActive) continue;
                            _continuousJogActive = true;
                            step = yCurrent;

                            var yMax = useImperial ? _grblViewModel.MaxDistanceY / 25.4 :
                                _grblViewModel.MaxDistanceY;
                            step = yMax - Math.Abs(yCurrent);

                            command = FormattableString.Invariant($"$J = G91{mode}Y{-step}F{_grblViewModel.JogRate}");
                            ProcessJogCommand(command);
                            break;

                        // Step Section
                        //step X Mode Left
                        case GameInputGamepadButtons.LeftTriggerButton | GameInputGamepadButtons.DPadLeft:
                            _stepMode = true;
                            step = _grblViewModel.JogStep;
                            command = FormattableString.Invariant($"$J = G91{mode}X-{Math.Abs(step)}F{_grblViewModel.JogRate}"); ;
                            ProcessJogCommand(command);
                            break;

                        // step X Mode Right
                        case GameInputGamepadButtons.LeftTriggerButton | GameInputGamepadButtons.DPadRight:
                            _stepMode = true;
                            step = _grblViewModel.JogStep;
                            command = FormattableString.Invariant($"$J = G91{mode}X{step}F{_grblViewModel.JogRate}");
                            ProcessJogCommand(command);
                            break;

                        // step Y Mode Up
                        case GameInputGamepadButtons.LeftTriggerButton | GameInputGamepadButtons.DPadUp:
                            _stepMode = true;
                            step = _grblViewModel.JogStep;
                            command = FormattableString.Invariant($"$J = G91{mode}Y{step}F{_grblViewModel.JogRate}");
                            ProcessJogCommand(command);
                            break;

                        // step Y Mode Down
                        case GameInputGamepadButtons.LeftTriggerButton | GameInputGamepadButtons.DPadDown:
                            _stepMode = true;
                            step = _grblViewModel.JogStep;
                            command = FormattableString.Invariant($"$J = G91{mode}Y-{Math.Abs(step)}F{_grblViewModel.JogRate}");
                            ProcessJogCommand(command);
                            break;
                       
                        // step z Mode Down
                        case GameInputGamepadButtons.LeftTriggerButton | GameInputGamepadButtons.A:
                            _stepMode = true;
                            step = _grblViewModel.JogStep;
                            command = FormattableString.Invariant($"$J = G91{mode}Z-{Math.Abs(step)}F{_grblViewModel.JogRate}");
                            ProcessJogCommand(command);
                            break;
                       
                        // step Z Mode Down
                        case GameInputGamepadButtons.LeftTriggerButton | GameInputGamepadButtons.B:
                            _stepMode = true;
                            step = _grblViewModel.JogStep;
                            command = FormattableString.Invariant($"$J = G91{mode}Z{step}F{_grblViewModel.JogRate}");
                            ProcessJogCommand(command);
                            break;

                        case GameInputGamepadButtons.X:
                            ProcessSinglePressCommand("G10L20P0Y0");
                            break;

                        case GameInputGamepadButtons.Y:
                            ProcessSinglePressCommand("G10L20P0X0");
                            break;

                        case GameInputGamepadButtons.LeftShoulder:
                            ProcessJogDistance();
                            break;

                        case GameInputGamepadButtons.RightShoulder:
                            ProcessJogFeedRate();
                            break;
                        //case GamepadButtons.Menu:
                        //continue;
                        // break;
                        case GameInputGamepadButtons.View:
                            ProcessSinglePressCommand("G10L20P0Z0");
                            break;

                        case GameInputGamepadButtons.RightThumbstick:
                            ProcessSinglePressCommand(GrblConstants.CMD_HOMING);
                            break;

                        case GameInputGamepadButtons.LeftThumbstick:
                            ProcessSinglePressCommand(GrblConstants.CMD_UNLOCK);
                            break;
                    }

                    if (Math.Abs(state.LeftTrigger - 1) < 0.1)
                    {
                        _continuousJogActive = false;
                        _stepMode = true;

                    }
          
                    if (state.Buttons == GameInputGamepadButtons.None
                        && _previousDown != GameInputGamepadButtons.None
                        && !_stepMode)
                    {

                        Comms.com.WriteByte(GrblConstants.CMD_JOG_CANCEL);
                        _continuousJogActive = false;
                    }
                    //TODO JoyStick has too much drift when releasing 
                    //var x = Math.Round(input.LeftThumbstickX, 1);
                    //var y = Math.Round(input.LeftThumbstickY, 1);
                    // ProcessJoyStick(x, y);
                   // Debug.WriteLine($"Previous Down {_previousDown}");
                    _previousDown = state.Buttons;
                    
                }
            }

        }

        private void ProcessJoyStick(double x, double y)
        {
            if ((x + y) == 0 && !_joystickJogging) return;
            var commandX = x > 0 ? "X" : "X-";
            var commandY = y > 0 ? "Y" : "Y-";
            var absX = Math.Abs(x);
            var absY = Math.Abs(y);
            var command = ProcessVelocity(absX, commandX, absY, commandY);
            if (string.IsNullOrEmpty(command)) return;
            Console.WriteLine(command);
            Send(command);
        }

        private string ProcessVelocity(double velocityX, string commandX, double velocityY, string commandY)
        {
            string command;
            _joystickJogging = true;
            var feedRateX = BuildJoggingCommand(velocityX);
            var feedRateY = BuildJoggingCommand(velocityY);
            var averageRate = (feedRateX + feedRateY) / 2;

            if (feedRateX == 0)
            {
                var step = CalculateJogStep(feedRateY);
                command = $"{JogHeader}{commandY}{step}F{feedRateY}";
            }
            else if (feedRateY == 0)
            {
                var step = CalculateJogStep(feedRateX);
                command = $"{JogHeader}{commandX}{step}F{feedRateX}";
            }
            else
            {
                var step = CalculateJogStep(averageRate);
                command = $"{JogHeader}{commandX}{step}{commandY}{_grblViewModel.JogStep}F{averageRate}";
            }

            if (!averageRate.Equals(0)) return command;
            _joystickJogging = false;
            Comms.com.WriteByte(GrblConstants.CMD_JOG_CANCEL);
            command = string.Empty;
            _pollRate = 50;
            return command;
        }

        private double CalculateJogStep(double feedRate)
        {
            var step = 0.0;
            if (feedRate > 1500)
            {
                step = 1.65;
                _pollRate = 50;
            }
            else if (feedRate > 500)
            {
                step = 1.5;
                _pollRate = 110;

            }
            else
            {
                step = .35;
                _pollRate = 210;
            }
            return step;
        }

        private double BuildJoggingCommand(double velocity)
        {
            double feedRate = 0;
            //if (velocity >= .7)
            //{
            //    feedRate = _feedRate[3];
            //}
            //else if (velocity > .4)
            //{
            //    feedRate = _feedRate[2];
            //}
            //else if (velocity > .3)
            //{
            //    feedRate = _feedRate[1];
            //}
            if (velocity > .3)
            {
                feedRate = _grblViewModel.JogRate;
            }
            else if (velocity <= .3)
            {
                feedRate = 0;
            }
            return feedRate;
        }


        // Single Axis Joystick movement 
        // For using joystick for JogMetric found to much drift on release of joystick causing machine to jog and appearance of latency 
        private void ProcessX(double movement)
        {
            if (movement == 0 && !_joystickJogging) return;
            var command = movement > 0 ? "$J = G91G21X" : "$J = G91G21X-";
            var x = Math.Abs(movement);
            var c = ProcessVelocity(x, command);
            if (!string.IsNullOrEmpty(c))
            {
                Send(c);
            }
        }
        // Single Axis Joystick movement 
        // For using joystick for JogMetric found to much drift on release of joystick causing machine to jog and appearance of latency
        private void ProcessY(double movement)
        {
            if (movement == 0 && !_joystickJogging) return;
            var command = movement > 0 ? $"$J = G91G21Y" : $"$J = G91G21Y-";
            var y = Math.Abs(movement);
            command += ProcessVelocity(y, command);
            if (!string.IsNullOrEmpty(command))
            {
                Send(command);
            }
        }
        //// Single Axis Joystick movement 
        private string ProcessVelocity(double velocity, string command)
        {
            _joystickJogging = true;

            if (velocity > .8)
            {
                command += $"{_grblViewModel.JogStep}F{_feedRate[3]}";
            }
            else if (velocity > .5)
            {
                command += $"{_grblViewModel.JogStep}F{_feedRate[2]}";
            }
            else if (velocity > .2)
            {
                command += $"{_grblViewModel.JogStep}F{_feedRate[1]}";
            }
            else if (velocity <= .2)
            {
                Comms.com.WriteByte(GrblConstants.CMD_JOG_CANCEL);
                command = string.Empty;
                _joystickJogging = false;
            }
            return command;
        }

        private void ProcessJogCommand(string command)
        {
            //Send(command);
            if (!_stepMode)
            {
                Send(command);
            }
            else
            {
                if (_jogProcessed) return;
                Debug.WriteLine(command);
                Send(command);
                _jogProcessed = true;
            }
        }
        private void Send(string command)
        {
            Comms.com.WriteCommand(command);
        }

        private void ProcessSinglePressCommand(string command)
        {
            if (_singleActionPress) return;
            {
                Send(command);
            }
            _singleActionPress = true;
        }

        private void ProcessJogDistance()
        {
            if (_singleActionPress) return;
            if (JogStepRate != JogStep.Step3)
            {
                JogStepRate += 1;
            }
            else
            {
                JogStepRate = JogStep.Step0;
            }
            SetJogDistance();
            _singleActionPress = true;
        }
        private void ProcessJogFeedRate()
        {
            if (_singleActionPress) return;
            if (JogFeedRate != JogFeed.Feed3)
            {
                JogFeedRate += 1;
            }
            else
            {
                JogFeedRate = JogFeed.Feed0;
            }
            SetJogRate();
            _singleActionPress = true;
        }
        private void SetJogRate()
        {
            _grblViewModel.JogRate = FeedRate;
        }
        private void SetJogDistance()
        {
            _grblViewModel.JogStep = DistanceRate;
        }

        private void ReleaseUnmanagedResources()
        {
            _cancellationTokenSource.Cancel();
        }

        private void Dispose(bool disposing)
        {
            try
            {
                ReleaseUnmanagedResources();
                if (!disposing) return;
                if (_buttonPollThread.Status is TaskStatus.RanToCompletion or TaskStatus.Canceled )
                {
                    _buttonPollThread?.Dispose();
                }
              
                _cancellationTokenSource?.Dispose();
            }
            catch (Exception e)
            {
                //
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }


   

}
