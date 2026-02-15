namespace GrblHalSender.GrblCore;

public class Tool : Position
{
    public Tool(string code) : base()
    {
        Code = code;
    }

    public Tool(string code, string offsets) : base(offsets)
    {
        Code = code;
    }

    public string Code { get; set; }

    double _r;

    public double R { get { return R; } set { _r = value; OnPropertyChanged(); } }

    public new string ToString(AxisFlags axisflags, int precision = 3)
    {
        return "P" + Code + base.ToString(axisflags, precision);
    }
}