using GrblHalSender.ViewModels;

namespace GrblHalSender.GrblCore;

public static class GrblParserState
{
    private static string _tool = string.Empty;
    private static Dictionary<string, string> state = new Dictionary<string, string>();

    public static bool Get(GrblViewModel model)
    {
        bool? res = null;
        CancellationToken cancellationToken = new CancellationToken();

        PollGrbl.Suspend();

        new Thread(() =>
        {
            res = WaitFor.AckResponse<string>(
                cancellationToken,
                response => Process(response),
                a => model.OnResponseReceived += a,
                a => model.OnResponseReceived -= a,
                400, () => Comms.com.WriteCommand(GrblConstants.CMD_GETPARSERSTATE));
        }).Start();

        while (res == null)
            EventUtils.DoEvents();

        PollGrbl.Resume();

        return res == true;
    }

    public static bool Get()
    {
        return Grbl.GrblViewModel != null && Get(Grbl.GrblViewModel);
    }

    /* Vanilla grbl workaround */

    public static bool Get(bool addMissing)
    {
        if (addMissing && (addMissing = Grbl.GrblViewModel != null && Get(Grbl.GrblViewModel) && GrblWorkParameters.Get()))
        {
            if (IsActive("G43.1") == null || IsActive("G49") == null)
            {
                var isOffset = IsPositionOffset(GrblWorkParameters.ToolLengtOffset);
                if ((isOffset ? IsActive("G43.1") : IsActive("G49")) == null)
                    state.Add(isOffset ? "G43.1" : "G49", "");
            }

            if (IsActive("G92") == null)
            {
                var g92 = GrblWorkParameters.GetCoordinateSystem("G92");
                if (g92 != null && IsPositionOffset(g92))
                    state.Add("G92", "");
            }
        }

        return addMissing;
    }

    private static bool IsPositionOffset(Position pos)
    {
        bool isOffset = false;

        foreach (int i in GrblInfo.AxisFlags.ToIndices())
            isOffset |= !(double.IsNaN(pos.Values[i]) || pos.Values[i] != 0d);

        return isOffset;
    }

    /* End vanilla grbl workaround */

    public static string Tool
    {
        get { return _tool; }
        set
        {
            _tool = value;
            if (GrblWorkParameters.Tools.Where(t => t.Code == _tool.ToString()).FirstOrDefault() == null)
                GrblWorkParameters.Tools.Add(new Tool(_tool.ToString()));
        }
    }
    public static string WorkOffset { get; set; }
    public static bool IsLoaded { get { return state.Count > 0; } }

    public static SpindleState SpindleState { get; private set; } = SpindleState.Off;
    public static CoolantState CoolantState { get; private set; } = CoolantState.Off;
    public static MotionMode MotionMode { get; private set; } = MotionMode.G0;
    public static FeedRateMode FeedRateMode { get; private set; } = FeedRateMode.UnitsPerMin;
    public static DistanceMode DistanceMode { get; private set; } = DistanceMode.Absolute;
    public static LatheMode LatheMode { get; set; } = LatheMode.Disabled;
    public static IJKMode IJKMode { get; private set; } = IJKMode.Incremental;
    public static ToolLengthOffset ToolLengthOffset { get; private set; } = ToolLengthOffset.Cancel;
    public static Units Units { get; private set; } = Units.Metric;
    public static bool IsMetric { get { return Units == Units.Metric; } }
    public static Plane Plane { get; private set; } = Plane.XY;
    public static bool IsScalingActive { get; private set; } = false;

    public static string IsActive(string key) // returns null if not active, "" or parsed value if not
    {
        string value = null;

        state.TryGetValue(key, out value);

        return value;
    }

    public static bool Process(string data)
    {
        if (data.StartsWith("[GC:")) try
            {
                state.Clear();
                string[] s = data.Substring(4).TrimEnd(']').Split(' ');
                foreach (string val in s)
                {
                    if (val.StartsWith("G51"))
                        state.Add(val.Substring(0, 3), val.Substring(4));
                    else if (val.StartsWith("G5") && val.Length > 2 && "G54G55G56G57G58G59".Contains(val.Substring(0, 3)))
                        WorkOffset = val;
                    else if ("FST".Contains(val.Substring(0, 1)))
                    {
                        state.Add(val.Substring(0, 1), val.Substring(1));
                        if (val.Substring(0, 1) == "T")
                        {
                            _tool = val.Substring(1);

                            if (_tool == "0")
                                _tool = GrblConstants.NO_TOOL;
                        }
                    }
                    else
                    {
                        state.Add(val, "");
                        switch (val)
                        {
                            case "G0":
                                MotionMode = MotionMode.G0;
                                break;

                            case "G1":
                                MotionMode = MotionMode.G1;
                                break;

                            case "G2":
                                MotionMode = MotionMode.G2;
                                break;

                            case "G3":
                                MotionMode = MotionMode.G3;
                                break;

                            case "G5":
                                MotionMode = MotionMode.G5;
                                break;

                            case "G7":
                                LatheMode = LatheMode.Diameter;
                                break;

                            case "G8":
                                LatheMode = LatheMode.Radius;
                                break;

                            case "G17":
                                Plane = Plane.XY;
                                break;

                            case "G18":
                                Plane = Plane.XZ;
                                break;

                            case "G19":
                                Plane = Plane.YZ;
                                break;

                            case "G20":
                                Units = Units.Imperial;
                                break;

                            case "G21":
                                Units = Units.Metric;
                                break;

                            case "G33":
                                MotionMode = MotionMode.G33;
                                break;

                            case "G38.2":
                                MotionMode = MotionMode.G38_2;
                                break;

                            case "G38.3":
                                MotionMode = MotionMode.G38_3;
                                break;

                            case "G38.4":
                                MotionMode = MotionMode.G38_4;
                                break;

                            case "G38.5":
                                MotionMode = MotionMode.G38_5;
                                break;

                            case "G43":
                                ToolLengthOffset = ToolLengthOffset.Enable;
                                break;

                            case "G43.1":
                                ToolLengthOffset = ToolLengthOffset.EnableDynamic;
                                break;

                            case "G43.2":
                                ToolLengthOffset = ToolLengthOffset.ApplyAdditional;
                                break;

                            case "G49":
                                ToolLengthOffset = ToolLengthOffset.Cancel;
                                break;

                            case "G50":
                            case "G51":
                                IsScalingActive = val == "G51";
                                break;

                            case "G73":
                                MotionMode = MotionMode.G73;
                                break;

                            case "G76":
                                MotionMode = MotionMode.G76;
                                break;

                            case "G80":
                                MotionMode = MotionMode.G80;
                                break;

                            case "G81":
                                MotionMode = MotionMode.G81;
                                break;

                            case "G82":
                                MotionMode = MotionMode.G82;
                                break;

                            case "G83":
                                MotionMode = MotionMode.G83;
                                break;

                            case "G84":
                                MotionMode = MotionMode.G84;
                                break;

                            case "G85":
                                MotionMode = MotionMode.G85;
                                break;

                            case "G86":
                                MotionMode = MotionMode.G86;
                                break;

                            case "G87":
                                MotionMode = MotionMode.G87;
                                break;

                            case "G88":
                                MotionMode = MotionMode.G88;
                                break;

                            case "G89":
                                MotionMode = MotionMode.G89;
                                break;

                            case "G90":
                                DistanceMode = DistanceMode.Absolute;
                                break;

                            case "G90.1": // not supported or reported by grbl
                                IJKMode = IJKMode.Absolute;
                                break;

                            case "G91":
                                DistanceMode = DistanceMode.Incremental;
                                break;

                            case "G91.1": // not reported by grbl, default state
                                IJKMode = IJKMode.Incremental;
                                break;

                            case "G93":
                                FeedRateMode = FeedRateMode.InverseTime;
                                break;

                            case "G94":
                                FeedRateMode = FeedRateMode.UnitsPerMin;
                                break;

                            case "G95":
                                FeedRateMode = FeedRateMode.UnitsPerRev;
                                break;

                            case "M3":
                                SpindleState = SpindleState.CW;
                                break;

                            case "M4":
                                SpindleState = SpindleState.CCW;
                                break;

                            case "M5":
                                SpindleState = SpindleState.Off;
                                break;

                            case "M7":
                                CoolantState |= CoolantState.Mist;
                                break;

                            case "M8":
                                CoolantState |= CoolantState.Flood;
                                break;

                            case "M9":
                                CoolantState = CoolantState.Off;
                                break;
                        }
                    }
                }
                return true;
            }
            catch { }
        return false;
    }
}