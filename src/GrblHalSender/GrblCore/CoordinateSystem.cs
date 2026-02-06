using System.Globalization;

namespace GrblHalSender.GrblCore;

public class CoordinateSystem : Position
{
    string _code = string.Empty;

    public CoordinateSystem() : base()
    { }

    public CoordinateSystem(string code, string data) : base(data)
    {
        Code = code;

        if (code.StartsWith("G5"))
        {
            double id = Math.Round(double.Parse(code.Substring(2), CultureInfo.InvariantCulture) - 3.0d, 1);

            Id = (int)Math.Floor(id) + (int)Math.Round((id - Math.Floor(id)) * 10.0d, 0);
        }
    }

    public int Id { get; private set; }
    public string Code { get { return _code; } set { _code = value; OnPropertyChanged(); } }

}