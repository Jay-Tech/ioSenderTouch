namespace GrblHalSender.GrblCore;

public class CoordinateValues<T> : ViewModelBase
{
    private bool _suspend = false;
    private T[] arr = new T[9];

    public int Length { get { return 9; } }
    public bool SuspendNotifications
    {
        get { return _suspend; }
        set
        {
            if (!(_suspend = value))
            {
                //for(int i = 0; i < Length; i++)
                //  //  if(!double.IsNaN((double)arr[i]))
                //        OnPropertyChanged(GrblInfo.AxisLetters.Substring(i, 1));
            }
        }
    }

    public T[] Array { get { return arr; } }

    public T this[int i]
    {
        get { return arr[i]; }
        set
        {
            if (!value.Equals(arr[i]))
            {
                arr[i] = value;
                if (!_suspend)
                    OnPropertyChanged("XYZABCUVW".Substring(i, 1));
            }
        }
    }
}