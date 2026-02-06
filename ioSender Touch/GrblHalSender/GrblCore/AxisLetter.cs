using System.ComponentModel;

namespace GrblHalSender.GrblCore;

public class AxisLetter : ViewModelBase
{
    public AxisLetter()
    {
        Values.PropertyChanged += Values_PropertyChanged;

        Remap(GrblInfo.AxisLetters);
    }

    private void Values_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(e.PropertyName);
    }

    public void Remap(string map)
    {
        int i;
        for (i = 0; i < map.Length; i++)
            Values[i] = map.Substring(i, 1);

        for (; i < Values.Length; i++)
            Values[i] = "-";
    }

    public string All
    {
        get
        {
            string all = string.Empty;
            for (int i = 0; i < Values.Length; i++)
                all += Values[i];

            return all;
        }
    }

    public CoordinateValues<string> Values { get; private set; } = new CoordinateValues<string>();
    public string X { get { return Values[0]; } set { Values[0] = value; } }
    public string Y { get { return Values[1]; } set { Values[1] = value; } }
    public string Z { get { return Values[2]; } set { Values[2] = value; } }
    public string A { get { return Values[3]; } set { Values[3] = value; } }
    public string B { get { return Values[4]; } set { Values[4] = value; } }
    public string C { get { return Values[5]; } set { Values[5] = value; } }
    public string U { get { return Values[6]; } set { Values[6] = value; } }
    public string V { get { return Values[7]; } set { Values[7] = value; } }
    public string W { get { return Values[8]; } set { Values[8] = value; } }
}