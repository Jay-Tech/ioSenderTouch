using System.ComponentModel;

namespace GrblHalSender.GrblCore;

public class Position : ViewModelBase
{
    public Position()
    {
        init();
    }

    public Position(string values)
    {
        init();
        Parse(values);
    }

    public Position(double x, double y, double z)
    {
        init();
        X = x;
        Y = y;
        Z = z;
    }

    public Position(Position pos)
    {
        init();
        for (var i = 0; i < Values.Length; i++)
            Values[i] = pos.Values[i];
    }

    public Position(Position pos, double scaleFactor)
    {
        init();
        foreach (int i in GrblInfo.AxisFlags.ToIndices())
            Values[i] = pos.Values[i] * scaleFactor;

        if (double.IsNaN(Y))
            Y = pos.Y;
    }

    private void init()
    {
        for (var i = 0; i < Values.Length; i++)
            Values[i] = double.NaN;

        Name = GetType().Name;
        Values.PropertyChanged += Values_PropertyChanged;
    }

    public void Clear()
    {
        bool changed = false;

        for (var i = 0; i < Values.Length; i++)
        {
            changed |= !Values[i].Equals(double.NaN);
            Values[i] = double.NaN;
        }

        if (changed && !Values.SuspendNotifications)
            OnPropertyChanged(nameof(Position));
    }

    public void Zero()
    {
        bool changed = false;

        for (var i = 0; i < Values.Length; i++)
        {
            changed |= !Values[i].Equals(0d);
            Values[i] = 0d;
        }

        if (changed && !Values.SuspendNotifications)
            OnPropertyChanged(nameof(Position));
    }

    public static Position operator +(Position b, Position c)
    {
        Position a = new Position();

        for (var i = 0; i < a.Values.Length; i++)
            a.Values[i] = b.Values[i] + c.Values[i];

        return a;
    }

    public static Position operator -(Position b, Position c)
    {
        Position a = new Position();

        for (var i = 0; i < a.Values.Length; i++)
            a.Values[i] = b.Values[i] - c.Values[i];

        return a;
    }

    public void Add(Position pos)
    {
        foreach (int i in GrblInfo.AxisFlags.ToIndices())
            Values[i] += pos.Values[i];

        if (!Values.SuspendNotifications)
            OnPropertyChanged(nameof(Position));
    }

    public void Subtract(Position pos)
    {
        foreach (int i in GrblInfo.AxisFlags.ToIndices())
            Values[i] -= pos.Values[i];

        if (!Values.SuspendNotifications)
            OnPropertyChanged(nameof(Position));
    }

    public void Set(Position pos)
    {
        foreach (int i in GrblInfo.AxisFlags.ToIndices())
        {
            if (!Values[i].Equals(pos.Values[i]))
                Values[i] = pos.Values[i];
        }

        if (!Values.SuspendNotifications)
            OnPropertyChanged(nameof(Position));
    }

    public void Scale(double factor)
    {
        foreach (int i in GrblInfo.AxisFlags.ToIndices())
            Values[i] *= factor;

        if (factor != 1d && !Values.SuspendNotifications)
            OnPropertyChanged(nameof(Position));
    }

    public bool IsSet(AxisFlags axisflags)
    {
        bool ok = true;

        foreach (int i in axisflags.ToIndices())
            ok = ok && !double.IsNaN(Values[i]);

        return ok;
    }

    public bool Equals(Position pos)
    {
        bool equal = true;

        foreach (int i in GrblInfo.AxisFlags.ToIndices())
        {
            if (!(equal = Values[i].Equals(pos.Values[i])))
                break;
        }

        return equal;
    }

    public string Name { get; private set; }

    public bool SuspendNotifications
    {
        get { return Values.SuspendNotifications; }
        set { Values.SuspendNotifications = value; }
    }

    private void Values_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(e.PropertyName);
    }

    public bool Parse(string values)
    {
        bool changed = false, ok = false;

        try
        {
            double[] position = dbl.ParseList(values);

            for (var i = 0; i < position.Length; i++)
            {
                if (double.IsNaN(Values[i]) ? !double.IsNaN(position[i]) : Values[i] != position[i])
                {
                    Values[i] = position[i];
                    changed = true;
                }
            }

            if (changed && !Values.SuspendNotifications)
                OnPropertyChanged(nameof(Position));

            ok = true;
        }
        catch { }

        return ok;
    }

    public string ToString(AxisFlags axisflags, int precision = 3)
    {
        string parameters = string.Empty;

        foreach (int i in axisflags.ToIndices())
            parameters += GrblInfo.AxisIndexToLetter(i) + (Math.Round(Values[i], precision).ToInvariantString());

        return parameters;
    }
    public string ToString(AxisFlags axisflags, Direction direction, int precision = 3)
    {
        string parameters = string.Empty;

        foreach (int i in axisflags.ToIndices())
            parameters += GrblInfo.AxisIndexToLetter(i) + (Math.Round(direction == Direction.Negative ? -Values[i] : Values[i], precision).ToInvariantString());

        return parameters;
    }
    public string ToString(AxisFlags axisflags, AxisFlags toNegative, int precision = 3)
    {
        string parameters = string.Empty;

        foreach (int i in axisflags.ToIndices())
            parameters += GrblInfo.AxisIndexToLetter(i) + (Math.Round(toNegative.HasFlag(GCodeParser.AxisFlag[i]) ? -Values[i] : Values[i], precision).ToInvariantString());

        return parameters;
    }

    public CoordinateValues<double> Values { get; private set; } = new CoordinateValues<double>();
    public double X { get { return Values[0]; } set { Values[0] = value; } }
    public double Y { get { return Values[1]; } set { Values[1] = value; } }
    public double Z { get { return Values[2]; } set { Values[2] = value; } }
    public double A { get { return Values[3]; } set { Values[3] = value; } }
    public double B { get { return Values[4]; } set { Values[4] = value; } }
    public double C { get { return Values[5]; } set { Values[5] = value; } }
    public double U { get { return Values[6]; } set { Values[6] = value; } }
    public double V { get { return Values[7]; } set { Values[7] = value; } }
    public double W { get { return Values[8]; } set { Values[8] = value; } }
}