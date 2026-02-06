using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using GrblHalSender.ViewModels;

namespace GrblHalSender.GrblCore;

public static class GrblSettings
{
    private static List<string> responses = new List<string>();

    public static ObservableCollection<GrblSettingDetails> Settings { get; private set; } = new ObservableCollection<GrblSettingDetails>();

    public static bool IsLoaded { get { return Settings.Count > 0; } }
    public static bool ReportProbeCoordinates { get; private set; }

    public static GrblSettingDetails Get(GrblSetting key)
    {
        return Settings.Where(x => x.Id == ((int)key)).FirstOrDefault();
    }
    public static GrblSettingDetails Get(grblHALSetting key)
    {
        return Settings.Where(x => x.Id == ((int)key)).FirstOrDefault();
    }
    public static bool HasSetting(GrblSetting key)
    {
        return GetString(key) != null;
    }
    public static bool HasSetting(grblHALSetting key)
    {
        return GetString(key) != null;
    }

    public static string GetString(grblHALSetting key)
    {
        var setting = Settings.Where(x => x.Id == ((int)key)).FirstOrDefault();

        return setting != null ? setting.Value : null;
    }

    public static string GetString(GrblSetting key)
    {
        var setting = Settings.Where(x => x.Id == ((int)key)).FirstOrDefault();

        return setting != null ? setting.Value : null;
    }

    public static double GetDouble(GrblSetting key)
    {
        return dbl.Parse(GetString(key));
    }

    public static double GetDouble(grblHALSetting key)
    {
        return dbl.Parse(GetString(key));
    }

    public static int GetInteger(GrblSetting key)
    {
        int res = -1;
        var v = GetString(key);

        if (v != null)
            int.TryParse(v, out res);

        return res;
    }

    public static int GetInteger(grblHALSetting key)
    {
        int res = -1;
        var v = GetString(key);

        if (v != null)
            int.TryParse(v, out res);

        return res;
    }

    public static bool Load(GrblViewModel model)
    {
        bool? res = null;
        bool load, getExtended = !Resources.IsLegacyController && GrblInfo.IsGrblHAL && GrblInfo.Build >= 20200716;
        CancellationToken cancellationToken = new CancellationToken();

        PollGrbl.Suspend();
        model.Silent = true;

        if ((load = Settings.Count == 0) && GrblInfo.HasEnums)
        {
            new Thread(() =>
            {
                res = WaitFor.AckResponse<string>(
                    cancellationToken,
                    response => ProcessDetail(response),
                    a => model.OnResponseReceived += a,
                    a => model.OnResponseReceived -= a,
                    1000, () => Comms.com.WriteCommand(GrblConstants.CMD_GETSETTINGSDETAILS));
            }).Start();

            while (res == null)
                EventUtils.DoEvents();

            GrblSettingGroups.Get(model);
        }

        foreach (var response in responses)
            Settings.Add(new GrblSettingDetails(response));

        res = null;
        responses.Clear();

        new Thread(() =>
        {
            res = WaitFor.AckResponse<string>(
                cancellationToken,
                response => Process(response),
                a => model.OnResponseReceived += a,
                a => model.OnResponseReceived -= a,
                1000, () => Comms.com.WriteCommand(getExtended ? GrblConstants.CMD_GETSETTINGS_ALL : GrblConstants.CMD_GETSETTINGS));
        }).Start();

        while (res == null)
            EventUtils.DoEvents();

        model.Silent = false;
        PollGrbl.Resume();

        if (load)
        {
            GrblSettingGroups.RemoveUnused();

            if (!GrblInfo.IsGrblHAL || !GrblInfo.HasSettingDescriptions || Resources.Locale != "en-US")
                try
                {
                    StreamReader file;
                    string filename = string.Format("{0}{1}setting_codes_{2}.txt",
                        Resources.Path,
                        GrblInfo.IsGrblHAL ? "hal_" : string.Empty,
                        Resources.Locale == "en-US" ? "en_US" : Resources.Locale);

                    file = FileUtils.OpenFile(filename);

                    if (file == null && Resources.Locale != "en-US")
                        file = FileUtils.OpenFile(filename.Replace(Resources.Locale, "en_US"));

                    if (file != null)
                    {
                        string line = file.ReadLine();

                        line = file.ReadLine(); // Skip header  

                        while (line != null)
                        {
                            string[] values = line.Split('\t');

                            if (values.Length >= 6)
                            {
                                var setting = Settings.Where(x => x.Id == int.Parse(values[0])).FirstOrDefault();

                                if (setting != null)
                                {

                                    if (setting.Name == string.Empty)
                                    {
                                        try
                                        {
                                            setting.DataType = (GrblSettingDetails.DataTypes)Enum.Parse(typeof(GrblSettingDetails.DataTypes), values[3].ToUpperInvariant());
                                        }
                                        catch
                                        {
                                            setting.DataType = GrblSettingDetails.DataTypes.TEXT;
                                        }

                                        setting.Name = values[1];
                                        setting.Format = values[4];

                                        if (setting.DataType == GrblSettingDetails.DataTypes.INTEGER || setting.DataType == GrblSettingDetails.DataTypes.FLOAT)
                                            setting.Unit = values[2];

                                        if (values.Length > 6)
                                            setting.Min = values[6] == string.Empty ? double.NaN : dbl.Parse(values[6]);

                                        if (values.Length > 7)
                                            setting.Max = values[7] == string.Empty ? double.NaN : dbl.Parse(values[7]);
                                    }
                                    else if (Resources.Locale != "en-US")
                                    {
                                        setting.Name = values[1];

                                        if (setting.DataType == GrblSettingDetails.DataTypes.INTEGER || setting.DataType == GrblSettingDetails.DataTypes.FLOAT)
                                            setting.Unit = values[2];
                                    }

                                    setting.Description = values[5].Replace("\\n", "\r\n");
                                }
                            }
                            line = file.ReadLine();
                        }
                        file.Close();
                        file.Dispose();
                    }
                }
                catch (Exception ex)
                {
                }
        }

        if (!GrblInfo.IsGrblHAL)
            ReportProbeCoordinates = true;

        GrblInfo.OnSettingsLoaded(model);
        model.SettingsLoaded();
        return IsLoaded;
    }

    public static bool Load()
    {
        return Grbl.GrblViewModel != null && Load(Grbl.GrblViewModel);
    }


    public static bool HasChanges()
    {
        var changed = Settings.Where(x => x.IsDirty);

        return changed != null && changed.Count() > 0;
    }

#if USE_ASYNC
        public static async void Save()
#else
    public static bool Save()
#endif
    {
        bool ok = true;
        var changed = Settings.Where(x => x.IsDirty);

        if (changed != null && changed.Count() > 0)
        {
            foreach (var setting in changed)
            {
#if USE_ASYNC
                    var task = Task.Run(() => Comms.com.AwaitAck(string.Format("${0}={1}", Setting.Id, Setting.Value)));
                    await await Task.WhenAny(task, Task.Delay(2500));
#else
                Comms.com.WriteCommand(string.Format("${0}={1}", setting.Id, setting.Value));
                Comms.com.AwaitAck();
#endif
                setting.ClearErrors();
                if (Comms.com.Reply.StartsWith("error:"))
                {
                    ok = false;
                    setting.SetError(GrblErrors.GetMessage(Comms.com.Reply.Substring(6)));
                }

                setting.IsDirty = setting.HasErrors;
            }
        }

        return ok;
    }

    private static List<string> Export()
    {
        List<string> exp = new List<string>();

        if (GrblInfo.IsGrblHAL)
            exp.Add("%");

        exp.Add("; " + GrblInfo.Firmware + (GrblInfo.Identity != string.Empty ? ":" + GrblInfo.Identity : ""));
        exp.Add("; " + GrblInfo.Version);
        exp.Add("; [OPT:" + GrblInfo.Options + "]");

        if (GrblInfo.NewOptions != string.Empty)
            exp.Add("; [NEWOPT:" + GrblInfo.NewOptions + "]");

        foreach (string opt in GrblInfo.SystemInfo)
            exp.Add("; " + opt);

        exp.Add(";");

        if (GrblInfo.Identity != string.Empty)
            exp.Add(string.Format("{0}={1}", GrblConstants.CMD_GETINFO, GrblInfo.Identity));

        if (GrblStartupLines.Get())
        {
            int id = 0;
            foreach (var line in GrblStartupLines.Lines)
            {
                exp.Add(string.Format("{0}{1}={2}", GrblConstants.CMD_GETSTARTUPLINES, id++, line));
            }
        }

        foreach (GrblSettingDetails setting in Settings)
        {
            if (!string.IsNullOrEmpty(setting.Name))
                exp.Add(string.Format("; {0} - {1}", setting.Id, setting.Name));
            exp.Add(string.Format("${0}={1}", setting.Id, setting.Value));
        }

        if (GrblInfo.IsGrblHAL)
            exp.Add("%");

        return exp;
    }

    public static void CopyToClipboard()
    {
        if (Settings.Count > 0) try
            {
                Clipboard.SetText(string.Join("\r\n", Export().ToArray()));
            }
            catch
            {
            }
    }

    public static void Backup(string filename)
    {
        if (Settings.Count > 0) try
            {
                StreamWriter file = new StreamWriter(filename);
                if (file != null)
                {
                    List<string> settings = Export();

                    foreach (string s in settings)
                        file.WriteLine(s);

                    file.Close();
                }
            }
            catch
            {
            }
    }

    public static string FormatFloat(string value, string format)
    {
        float fval;
        if (float.TryParse(value, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out fval))
            value = fval.ToString(format.StartsWith(NumberFormatInfo.CurrentInfo.NegativeSign) ? format.Substring(1) : format, CultureInfo.InvariantCulture);
        return value;
    }

    private static void ProcessDetail(string data)
    {
        if (data != "ok")
        {
            string[] valuepair = data.TrimEnd(']').Split(':');

            if (valuepair.Length == 2 && valuepair[0] == "[SETTING")
                responses.Add(valuepair[1]);
        }
    }

    private static void Process(string data)
    {
        if (data != "ok")
        {
            int id;
            string[] valuepair = data.Split('=');

            if (valuepair.Length == 2 && int.TryParse(valuepair[0].Substring(1), out id))
            {
                switch ((GrblSetting)id)
                {
                    case GrblSetting.HomingEnable: // TODO: remove?
                        GrblInfo.HomingEnabled = valuepair[1] != "0";
                        break;

                    case (GrblSetting)grblHALSetting.EnableLegacyRTCommands: // TODO: remove!
                        GrblInfo.UseLegacyRTCommands = valuepair[1] != "0";
                        break;

                    case (GrblSetting)grblHALSetting.JogStepDistance:
                        GrblInfo.HasFirmwareJog = true;
                        break;


                    case GrblSetting.StatusReportMask:
                    {
                        if (String.IsNullOrEmpty(valuepair[1])) // FluidNC workaround
                            valuepair[1] = "0";                 // for missing parameter value
                        var value = int.Parse(valuepair[1]);
                        Grbl.GrblViewModel.IsParserStateLive = (value & (1 << 9)) != 0;
                        GrblInfo.ReportProbeResult = ReportProbeCoordinates = (value & (1 << 7)) != 0;
                        GrblInfo.HasSimpleProbeProtect = (value & (1 << 11)) != 0;
                    }
                        break;
                }

                var setting = Settings.Where(x => x.Id == id).FirstOrDefault();

                if (setting == null)
                {
                    Action<GrblSettingDetails> addMethod = Settings.Add;
                    setting = new GrblSettingDetails(id.ToString() + "|0||||||");
                    Application.Current.Dispatcher.BeginInvoke(addMethod, setting);
                }

                setting.Value = valuepair[1];
                setting.Silent = setting.IsDirty = false;
            }
        }
    }
}