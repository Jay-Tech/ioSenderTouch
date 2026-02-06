using System.Collections.ObjectModel;
using System.Windows.Threading;
using GrblHalSender.ViewModels;

namespace GrblHalSender.GrblCore;

public class GrblWorkParameters
{
    private static Dispatcher dispatcher;
    public static bool IsLoaded { get { return CoordinateSystems.Count > 0; } }
    public static LatheMode LatheMode { get; private set; }
    public static double ToolLengthOffsetReference { get; private set; } = double.NaN;
    public static ObservableCollection<CoordinateSystem> CoordinateSystems { get; private set; } = new ObservableCollection<CoordinateSystem>();
    public static ObservableCollection<Tool> Tools { get; private set; } = new ObservableCollection<Tool>();
    public static CoordinateSystem ToolLengtOffset { get; private set; } = new CoordinateSystem("TLO", "");
    public static CoordinateSystem ProbePosition { get; private set; } = new CoordinateSystem("PRB", "");
    public static bool ProbeSuccesful { get; private set; } = false;

    private static Action<string> dataReceived;

    public static CoordinateSystem GetCoordinateSystem(string gCode)
    {
        return CoordinateSystems.Where(x => x.Code == gCode).FirstOrDefault();
    }

    public static double ConvertX(LatheMode source, LatheMode target, double value)
    {
        if (source != target) switch (target)
        {
            case LatheMode.Radius:
                value /= 2.0d;
                break;

            case LatheMode.Diameter:
                value *= 2.0d;
                break;
        }

        return value;
    }

    public static bool Get(GrblViewModel model)
    {
        bool? res = null;
        CancellationToken cancellationToken = new CancellationToken();

        if (Tools.Count == 0)
            Tools.Add(new Tool(GrblConstants.NO_TOOL));

        if (!GrblParserState.IsLoaded)
            GrblParserState.Get(model);

        dispatcher = Dispatcher.CurrentDispatcher;
        dataReceived += process;
        LatheMode = GrblParserState.LatheMode;

        model.Silent = true;

        PollGrbl.Suspend();

        new Thread(() =>
        {
            res = WaitFor.AckResponse<string>(
                cancellationToken,
                response => dataReceived(response),
                a => model.OnResponseReceived += a,
                a => model.OnResponseReceived -= a,
                400, () => Comms.com.WriteCommand(GrblConstants.CMD_GETNGCPARAMETERS));
        }).Start();

        while (res == null)
            EventUtils.DoEvents();

        PollGrbl.Resume();

        model.Silent = false;
        dataReceived -= process;

        if (Tools.Count == 1)
        {
            Tools.Add(new Tool("1"));
            Tools.Add(new Tool("2"));
            Tools.Add(new Tool("3"));
            Tools.Add(new Tool("4"));
        }

        //            GrblParserState.Tool = GrblParserState.Tool;    // Add tool to Tools if not in list
        model.Tool = model.Tool;                        // Force UI update
        model.ToolOffset.Z = ToolLengtOffset.Z;

        return res == true;
    }

    public static bool Get()
    {
        return Grbl.GrblViewModel != null && Get(Grbl.GrblViewModel);
    }

    private static string extractValues(string data, out string parameters)
    {
        int sep = data.IndexOf(":");
        if (sep > 0)
        {
            parameters = data.Substring(sep + 1).TrimEnd(']');
            return data.Substring(1, sep - 1);
        }
        parameters = "";
        return "";
    }

    public static void RemoveNoTool()
    {
        Tool tool = Tools.Where(x => x.Code == GrblConstants.NO_TOOL).FirstOrDefault();
        if (tool != null)
            Tools.Remove(tool);
    }

    private static void AddOrUpdateTool(string gCode, string data)
    {
        string[] s1 = data.Split('|');
        string[] s2 = s1[1].Split(',');

        Tool tool = Tools.Where(x => x.Code == s1[0]).FirstOrDefault();
        if (tool == null)
        {
            tool = new Tool(s1[0], s1[1]);
            Tools.Add(tool);

        }
        else
            tool.Parse(s1[1]);

        if (s1.Length > 2)
        {
            s2 = s1[2].Split(',');
            tool.R = dbl.Parse(s2[0]);
        }
    }

    private static CoordinateSystem AddOrUpdateCS(string gCode, string data)
    {
        CoordinateSystem cs = CoordinateSystems.FirstOrDefault(x => x.Code == gCode);
        if (cs == null)
            CoordinateSystems.Add(cs = new CoordinateSystem(gCode, data));
        else
            cs.Parse(data);

        return cs;
    }

    private static void process(string data)
    {
        if (Dispatcher.CurrentDispatcher != dispatcher)
        {
            dispatcher.Invoke(dataReceived, data);
            return;
        }

        if (data.StartsWith("["))
        {
            string parameters, gCode = extractValues(data, out parameters);
            switch (gCode)
            {
                case "G28":
                case "G30":
                case "G54":
                case "G55":
                case "G56":
                case "G57":
                case "G58":
                case "G59":
                case "G59.1":
                case "G59.2":
                case "G59.3":
                case "G92":
                    AddOrUpdateCS(gCode, parameters);
                    break;

                case "T":
                    AddOrUpdateTool(gCode, parameters);
                    break;

                case "TLO":
                    // Workaround for legacy grbl, it reports only one offset...
                    ToolLengtOffset.SuspendNotifications = true;
                    ToolLengtOffset.Z = double.NaN;
                    ToolLengtOffset.SuspendNotifications = false;
                    // End workaround   

                    ToolLengtOffset.Parse(parameters);

                    // Workaround for legacy grbl, copy X offset to Z (there is no info available for which axis...)
                    if (double.IsNaN(ToolLengtOffset.Z))
                    {
                        ToolLengtOffset.Z = ToolLengtOffset.X;
                        ToolLengtOffset.X = ToolLengtOffset.Y = 0d;
                    }
                    // End workaround

                    //AddOrUpdateCS("G43.1", parameters);
                    break;

                case "TLR":
                    double tlr;
                    if (double.TryParse(parameters, out tlr))
                        ToolLengthOffsetReference = tlr;
                    break;

                case "PRB":
                {
                    var p = parameters.Split(':');
                    if (p.Length == 2)
                    {
                        ProbePosition.Parse(p[0]);
                        ProbeSuccesful = p[1] == "1";
                    }
                }
                    break;
            }
        }
    }
}