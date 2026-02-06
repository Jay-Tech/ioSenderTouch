namespace GrblHalSender.GrblCore;

public class GrblSettingDetails : ViewModelBase
{
    public enum DataTypes
    {
        BOOL = 0,
        BITFIELD,
        XBITFIELD,
        RADIOBUTTONS,
        AXISMASK,
        INTEGER,
        FLOAT,
        TEXT,
        PASSWORD,
        IP4
    };

    private string _value, _description = null;
    internal bool Silent = true;

    public GrblSettingDetails(string data)
    {
        string[] values = data.Split('|');

        Id = int.Parse(values[0]);
        GroupId = int.Parse(values[1]);
        Name = values[2];
        Unit = values[3];
        DataType = values[4] == string.Empty ? DataTypes.TEXT : (DataTypes)int.Parse(values[4]);
        Format = values[5];
        Min = values[6] == string.Empty ? double.NaN : dbl.Parse(values[6]);
        Max = values[7] == string.Empty ? double.NaN : dbl.Parse(values[7]);
        if (values.Length > 8)
            RebootRequired = values[8] == "1";
        if (values.Length > 9)
            AllowNull = values[9] == "1";
    }

    public int Id { get; internal set; }
    public int GroupId { get; internal set; }
    public string Name { get; internal set; }
    public string Value
    {
        get { return _value; }
        set
        {
            if (DataType == DataTypes.FLOAT)
                value = GrblSettings.FormatFloat(value, Format);
            if (_value != value)
            {
                _value = value;
                if ((IsDirty = !Silent))
                {
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(FormattedValue));
                }
            }
        }
    }
    public string FormattedValue
    {
        get
        {
            if (_value == null)
                return "n/a";

            switch (DataType)
            {
                case DataTypes.BOOL:
                    return _value == "0" ? "false" : "true";

                case DataTypes.BITFIELD:
                    return _value == "0" ? "no" : _value;

                case DataTypes.XBITFIELD:
                    return _value == "0" ? "disabled" : string.Format("enabled ({0})", _value);

                case DataTypes.AXISMASK:
                    if (_value != "0")
                    {
                        int axes = int.Parse(_value), idx = 0;
                        string res = string.Empty;
                        while (axes != 0)
                        {
                            if ((axes & 0x01) != 0)
                                res += GrblInfo.AxisIndexToLetter(idx);
                            axes >>= 1; idx++;
                        }
                        return res;
                    }
                    return "no";

                case DataTypes.RADIOBUTTONS:
                    return Format.Split(',')[int.Parse(_value)];
            }

            return _value;
        }
    }

    public string Unit { get; internal set; } = string.Empty;
    public string Format { get; internal set; } = string.Empty;
    public DataTypes DataType { get; internal set; }
    public double Min { get; internal set; }
    public double Max { get; internal set; }
    public bool AllowNull { get; internal set; }
    public bool RebootRequired { get; internal set; }
    public string Description
    {
        get
        {
            if (_description == null)
            {
                if (Grbl.GrblViewModel == null)
                    _description = String.Empty;
                else
                {
                    bool? res = null;
                    CancellationToken cancellationToken = new CancellationToken();

                    PollGrbl.Suspend();
                    Grbl.GrblViewModel.Silent = true;

                    new Thread(() =>
                    {
                        res = WaitFor.AckResponse<string>(
                            cancellationToken,
                            response => ProcessDetail(response),
                            a => Grbl.GrblViewModel.OnResponseReceived += a,
                            a => Grbl.GrblViewModel.OnResponseReceived -= a,
                            400, () => Comms.com.WriteCommand("$SED=" + Id.ToString()));
                    }).Start();

                    while (res == null)
                        EventUtils.DoEvents();

                    if (_description == null)
                        _description = String.Empty;

                    Grbl.GrblViewModel.Silent = false;
                    PollGrbl.Resume();
                }
            }

            return _description;
        }
        internal set
        {
            _description = value;
        }
    }

    private void ProcessDetail(string data)
    {
        if (data != "ok" && data.StartsWith("[SETTINGDESCR:"))
        {
            int pos = data.IndexOf('|');
            _description = data.Substring(pos + 1).TrimEnd(']').Replace("\\n", "\r\n"); ;
        }
    }

    public bool IsDirty { get; internal set; } = false;
}