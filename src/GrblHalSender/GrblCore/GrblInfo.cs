using System.Collections.ObjectModel;
using System.Globalization;
using GrblHalSender.ViewModels;

namespace GrblHalSender.GrblCore;

public static class GrblInfo
{
    #region Attributes

    private static bool _probeProtect = false;
    private static int _numAxes;

    static GrblInfo()
    {
        NumAxes = 3;
    }

    public static string AxisLetters { get; private set; } = "XYZABCUVW";
    public static string SignalLetters { get; private set; } = "XYZABCUVWEPRDHSBTOMF"; // Keep in sync with Signals enum above!!
    public static string PositionFormatString { get; private set; } = string.Empty;
    public static string Version { get; private set; } = string.Empty;
    public static int Build { get; private set; } = 0;
    public static bool IsGrblHAL { get; internal set; }
    public static string Firmware { get; internal set; } = "Grbl";
    public static bool ExtendedProtocol { get; internal set; }
    public static string Identity { get; private set; } = string.Empty;
    public static string Options { get; private set; } = string.Empty;
    public static string NewOptions { get; private set; } = string.Empty;
    public static string TrinamicDrivers { get; private set; } = string.Empty;

    public static int SerialBufferSize { get; private set; } = 128;

    public static int PlanBufferSize { get; private set; } = 16;
    public static bool ReportProbeResult { get; internal set; } = false;
    public static bool ForceSetOrigin { get; private set; } = false;
    public static bool ExpressionsSupported { get; private set; } = false;
    public static int NumAxes
    {
        get { return _numAxes; }
        private set
        {
            _numAxes = LatheModeEnabled ? Math.Max(value, 3) : value;
            int flags = 0;
            PositionFormatString = string.Empty;
            for (int i = 0; i < _numAxes; i++)
            {
                flags = (flags << 1) | 0x01;
                if (!LatheModeEnabled || i != 1)
                    PositionFormatString += AxisIndexToLetter(i) + ": {" + i.ToString() + "}  ";
            }
            if (LatheModeEnabled && _numAxes == 3)
            {
                flags &= ~0x02;
                _numAxes--;
            }
            PositionFormatString = PositionFormatString.TrimEnd(' ');
            AxisFlags = (AxisFlags)flags;
        }
    }
    public static Position TravelResolution { get; private set; } = new Position();
    public static Position MaxTravel { get; private set; } = new Position();
    public static Signals OptionalSignals { get; private set; } = Signals.Off;
    public static AxisFlags AxisFlags { get; private set; } = AxisFlags.None;
    public static int NumTools { get; private set; } = 0;
    public static int NumFans { get; private set; } = 0;
    public static bool HasATC { get; private set; }
    public static bool HasEnums { get; private set; }
    public static bool HasSettingDescriptions { get; private set; }
    public static bool HasSimpleProbeProtect { get { return _probeProtect & IsGrblHAL && Build >= 20200924; } internal set { _probeProtect = value; } }
    public static bool ManualToolChange { get; private set; }
    public static bool HasSDCard { get; private set; }
    public static string UploadProtocol { get; private set; } = string.Empty;
    public static string IpAddress { get; private set; } = string.Empty;
    public static bool HasPIDLog { get; private set; }
    public static bool HasProbe { get; private set; } = true;
    public static bool HasRTC { get; private set; } = false;
    public static bool HomingEnabled { get; internal set; } = false;
    public static AxisFlags HomingDirection { get; internal set; } = AxisFlags.None;
    public static bool UseLegacyRTCommands { get; internal set; } = true;
    public static bool MPGMode { get; set; }
    public static bool HasFirmwareJog { get; internal set; } = false;
    public static bool LightBurnCluster { get; internal set; } = false;
    public static bool LatheModeEnabled
    {
        get { return GrblParserState.LatheMode != LatheMode.Disabled; }
        set { if (value && GrblParserState.LatheMode == LatheMode.Disabled) { GrblParserState.LatheMode = LatheMode.Radius; } }
    }
    public static ObservableCollection<string> SystemInfo { get; private set; } = new ObservableCollection<string>();
    public static bool IsLoaded { get; private set; }

    #endregion

    public static string AxisIndexToLetter(int index)
    {
        return AxisLetters.Substring(index, 1);
    }

    public static int AxisLetterToIndex(string letter)
    {
        return AxisLetters.IndexOf(letter);
    }

    public static int AxisLetterToIndex(char letter)
    {
        return AxisLetters.IndexOf(letter);
    }

    public static AxisFlags AxisIndexToFlag(int index)
    {
        return (AxisFlags)(1 << index);
    }

    public static AxisFlags AxisLetterToFlag(string letter)
    {
        return (AxisFlags)(1 << AxisLetters.IndexOf(letter));
    }

    public static AxisFlags AxisLetterToFlag(char letter)
    {
        return (AxisFlags)(1 << AxisLetters.IndexOf(letter));
    }

    public static bool Get(GrblViewModel model)
    {
        bool? res = null;
        bool getExtended = !Resources.IsLegacyController && ExtendedProtocol; // && Build >= 20201109;
        CancellationToken cancellationToken = new CancellationToken();

        PollGrbl.Suspend();
        SystemInfo.Clear();

        model.Silent = true;
        Firmware = model.Firmware;

        new Thread(() =>
        {
            res = WaitFor.AckResponse<string>(
                cancellationToken,
                response => Process(response),
                a => model.OnResponseReceived += a,
                a => model.OnResponseReceived -= a,
                1000, () => Comms.com.WriteCommand(getExtended ? GrblConstants.CMD_GETINFO_EXTENDED : GrblConstants.CMD_GETINFO));
        }).Start();

        while (res == null)
            EventUtils.DoEvents();

        model.Silent = false;
        PollGrbl.Resume();

        model.NumAxes = NumAxes;
        model.AxisEnabledFlags = AxisFlags;
        model.LatheModeEnabled = LatheModeEnabled;
        model.OptionalSignals.Value = OptionalSignals;
        IsLoaded = res == true;

        if (IsGrblHAL) // For now...
            Firmware = "grblHAL";

        model.Firmware = Firmware;
        model.HasFans = NumFans > 0;

        if (!Resources.IsLegacyController)
            IsGrblHAL = IsGrblHAL || Firmware == "grblHAL";

        return res == true;
    }

    internal static void OnSettingsLoaded(GrblViewModel model)
    {
        model.IsMetric = GrblSettings.GetInteger(GrblSetting.ReportInches) != 1;

        model.Keyboard.IsContinuousJoggingEnabled = IsGrblHAL;
        model.Keyboard.SoftLimits = GrblSettings.GetInteger(GrblSetting.SoftLimitsEnable) == 1;
        model.Keyboard.LimitSwitchesClearance = GrblSettings.GetDouble(GrblSetting.HomingPulloff);
        model.GrblState = model.GrblState; // Temporary hack to enable the Home button when homing is enabled

        HomingDirection = (AxisFlags)GrblSettings.GetInteger(GrblSetting.HomingDirMask);

        if (AxisFlags == AxisFlags.None)
        {
            int i = 0;
            double stepsmm;
            do
            {
                stepsmm = GrblSettings.GetDouble(GrblSetting.TravelResolutionBase + i);
                TravelResolution.Values[i++] = 1d / stepsmm;
                MaxTravel.Values[i++] = GrblSettings.GetDouble(GrblSetting.MaxTravelBase + i); ;
            } while (!double.IsNaN(stepsmm) && i < TravelResolution.Values.Length);
        }
        else foreach (int i in AxisFlags.ToIndices())
        {
            TravelResolution.Values[i] = 1d / GrblSettings.GetDouble(GrblSetting.TravelResolutionBase + i);
            MaxTravel.Values[i] = GrblSettings.GetDouble(GrblSetting.MaxTravelBase + i);
        }

        if (HasFirmwareJog)
        {
            //double val;
            //if (!(val = GrblSettings.GetDouble(grblHALSetting.JogStepDistance)).Equals(double.NaN))
            //    model.Keyboard.JogStepDistance = val;
            //if (!(val = GrblSettings.GetDouble(grblHALSetting.JogSlowDistance)).Equals(double.NaN))
            //    model.Keyboard.JogDistances[(int)KeypressHandler.JogMode.Slow] = val;
            //if (!(val = GrblSettings.GetDouble(grblHALSetting.JogFastDistance)).Equals(double.NaN))
            //    model.Keyboard.JogDistances[(int)KeypressHandler.JogMode.Fast] = val;
            //if (!(val = GrblSettings.GetDouble(grblHALSetting.JogStepSpeed)).Equals(double.NaN))
            //    model.Keyboard.JogFeedrates[(int)KeypressHandler.JogMode.Step] = val;
            //if (!(val = GrblSettings.GetDouble(grblHALSetting.JogSlowSpeed)).Equals(double.NaN))
            //    model.Keyboard.JogFeedrates[(int)KeypressHandler.JogMode.Slow] = val;
            //if (!(val = GrblSettings.GetDouble(grblHALSetting.JogFastSpeed)).Equals(double.NaN))
            //    model.Keyboard.JogFeedrates[(int)KeypressHandler.JogMode.Fast] = val;
        }
    }

    public static bool Get()
    {
        return Grbl.GrblViewModel != null && Get(Grbl.GrblViewModel);
    }

    public static string Startup(GrblViewModel model)
    {
        bool? res = null;
        int retries = 10;

        PollGrbl.Suspend();
        CancellationToken cancellationToken = new CancellationToken();

        while (retries-- > 0)
        {
            res = null;

            new Thread(() =>
            {
                res = WaitFor.SingleEvent<string>(
                    cancellationToken,
                    OnStartup,
                    a => model.OnResponseReceived += a,
                    a => model.OnResponseReceived -= a,
                    250, () => Comms.com.WriteByte(GrblConstants.CMD_STATUS_REPORT_ALL));
            }).Start();

            while (res == null)
                EventUtils.DoEvents();

            if (Comms.com.Reply.StartsWith("<"))
                retries = 0;
        }

        if (!ExtendedProtocol)
        {
            res = null;
            Comms.com.PurgeQueue();

            new Thread(() =>
            {
                res = WaitFor.SingleEvent<string>(
                    cancellationToken,
                    OnLegacyStartup,
                    a => model.OnResponseReceived += a,
                    a => model.OnResponseReceived -= a,
                    1500, () => Comms.com.WriteByte((byte)GrblConstants.CMD_STATUS_REPORT_LEGACY[0]));
            }).Start();

            while (res == null)
                EventUtils.DoEvents();
        }
        else if (!Resources.IsLegacyController)
            IsGrblHAL = model.Firmware == "grblHAL";

        PollGrbl.Resume();

        return Comms.com.Reply;
    }

    private static void DetectNumAxes(string rt_report)
    {
        var s = rt_report.Split('|');
        if (s.Length > 1)
        {
            var pos = s[1].Split(':');
            if ((pos[0] == "MPos" || pos[0] == "WPos") && NumAxes != pos[1].Split(',').Length)
            {
                NumAxes = pos[1].Split(',').Length;
                if (Grbl.GrblViewModel != null)
                {
                    Grbl.GrblViewModel.NumAxes = NumAxes;
                    Grbl.GrblViewModel.AxisEnabledFlags = AxisFlags;
                    Grbl.GrblViewModel.ClearPosition();
                    Grbl.GrblViewModel.LatheModeEnabled = LatheModeEnabled;
                    Grbl.GrblViewModel.ParseStatus(rt_report);
                }
            }
        }
    }

    private static void OnStartup(string data)
    {
        if ((ExtendedProtocol = data.StartsWith("<")))
            DetectNumAxes(data);
    }

    private static void OnLegacyStartup(string data)
    {
        if (data.StartsWith("<"))
            DetectNumAxes(data);
    }

    private static void Process(string data)
    {
        if (data.StartsWith("["))
        {
            string[] valuepair = data.Substring(1).TrimEnd(']').Split(':');

            switch (valuepair[0])
            {
                case "VER":
                    Version = valuepair[1];
                    if (valuepair.Count() > 2)
                        Identity = valuepair[2];
                    if (Version.LastIndexOf('.') > 0)
                    {
                        int build = 0;
                        int.TryParse(Version.Substring(Version.LastIndexOf('.') + 1), out build);
                        Build = build;
                    }
                    break;

                case "OPT":
                    Options = valuepair[1];
                    string[] s = Options.Split(',');
                    if (s[0].Contains('+'))
                        OptionalSignals |= Signals.SafetyDoor;
                    ForceSetOrigin = s[0].Contains('Z');
                    if (s.Length > 1)
                        PlanBufferSize = int.Parse(s[1], CultureInfo.InvariantCulture);
                    if (s.Length > 2)
                        Grbl.GrblViewModel.RxBufferSize = SerialBufferSize = int.Parse(s[2], CultureInfo.InvariantCulture);
                    if (s.Length > 3 && NumAxes != int.Parse(s[3], CultureInfo.InvariantCulture))
                    {
                        NumAxes = int.Parse(s[3], CultureInfo.InvariantCulture);
                        if (Grbl.GrblViewModel != null)
                            Grbl.GrblViewModel.ClearPosition();
                    }
                    if (s.Length > 4)

                        NumTools = int.Parse(s[4], CultureInfo.InvariantCulture);
                    if (Grbl.GrblViewModel != null) Grbl.GrblViewModel.HasToolTable = NumTools > 0;
                    break;

                case "AXS":
                    NumAxes = int.Parse(valuepair[1], CultureInfo.InvariantCulture);
                    AxisLetters = valuepair[2];
                    if (Grbl.GrblViewModel != null)
                    {
                        Grbl.GrblViewModel.AxisLetter.Remap(AxisLetters);
                        AxisLetters = Grbl.GrblViewModel.AxisLetter.All;
                        SignalLetters = AxisLetters + SignalLetters.Substring(9);
                        Grbl.GrblViewModel.ClearSignals();
                        NumAxes = Grbl.GrblViewModel.NumAxes;
                    }
                    break;

                case "NEWOPT":
                    NewOptions = valuepair[1];
                    string[] s2 = valuepair[1].Split(',');
                    foreach (string value in s2)
                    {
                        if (value.StartsWith("TMC="))
                            TrinamicDrivers = value.Substring(4);
                        else switch (value)
                        {
                            case "ENUMS":
                                HasEnums = true;
                                break;

                            case "EXPR":
                                ExpressionsSupported = true;
                                break;

                            case "TC":
                                ManualToolChange = true;
                                break;

                            case "ATC":
                                HasATC = true;
                                break;

                            case "RTC":
                                HasRTC = true;
                                break;

                            case "ETH":
                                break;

                            case "HOME":
                                HomingEnabled = true;
                                break;

                            case "SD":
                                HasSDCard = true;
                                break;

                            case "SED":
                                HasSettingDescriptions = true;
                                break;

                            case "YM":
                                if (UploadProtocol == string.Empty)
                                    UploadProtocol = "YModem";
                                break;

                            case "FTP":
                                UploadProtocol = "FTP";
                                break;

                            case "PID":
                                HasPIDLog = true;
                                break;

                            case "NOPROBE":
                                HasProbe = false;
                                break;

                            case "LATHE":
                                LatheModeEnabled = true;
                                break;

                            case "BD":
                                OptionalSignals |= Signals.BlockDelete;
                                break;

                            case "ES":
                                OptionalSignals |= Signals.EStop;
                                break;

                            case "MW":
                                OptionalSignals |= Signals.MotorWarning;
                                break;

                            case "OS":
                                OptionalSignals |= Signals.OptionalStop;
                                break;

                            case "RT+":
                            case "RT-":
                                UseLegacyRTCommands = false;
                                break;
                        }
                    }
                    break;

                case "FIRMWARE":
                    Firmware = valuepair[1];
                    SystemInfo.Add(data);
                    break;

                case "CLUSTER":
                    LightBurnCluster = true;
                    SystemInfo.Add(data);
                    break;

                default:
                    SystemInfo.Add(data);
                    if (data.StartsWith("[AUX IO:"))
                        GrblAuxIO.ParseConfig(data);
                    else if (data.StartsWith("[FANS:"))
                        NumFans = int.Parse(data.Substring(6).TrimEnd(']'));
                    else if (data.StartsWith("[IP:"))
                        IpAddress = data.Substring(4).TrimEnd(']');
                    break;
            }
        }
    }
}