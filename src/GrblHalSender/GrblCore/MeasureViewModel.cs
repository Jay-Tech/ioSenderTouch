

namespace GrblHalSender.GrblCore
{
    public enum Measurement
    {
        Metric,
        Imperial,
    }
    public class MeasureViewModel : ViewModelBase
    {
       
        public event EventHandler<Measurement> GrblUnitChanged;
        bool _isMetric = true;

        public const double MM_PER_INCH = 25.4d;
       
        public bool IsMetric
        {
            get { return _isMetric; }
            set
            {
                if (value != _isMetric)
                {
                    _isMetric = value;
                    OnPropertyChanged("Unit");
                    OnPropertyChanged("FeedrateUnit");
                    OnPropertyChanged("UnitFactor");
                    OnPropertyChanged("Format");
                    OnPropertyChanged("FormatSigned");
                    OnPropertyChanged();
                    GrblUnitChanged?.Invoke(this, value? Measurement.Metric: Measurement.Imperial );
                }
            }
        }

        public string Unit { get { return _isMetric ? "mm" : "in"; } }
        public string FeedrateUnit { get { return _isMetric ? "mm/min" : "in/min"; } }
        public double UnitFactor { get { return _isMetric ? 1.0d : 25.4d; } }
        public string Format { get { return _isMetric ? GrblConstants.FORMAT_METRIC : GrblConstants.FORMAT_IMPERIAL; } }
        public string FormatSigned { get { return "-" + Format; } }
        public int Precision { get { return _isMetric ? 3 : 4; } }

        public double ConvertMM2Current (double value)
        {
            if(!_isMetric)
                value /= 25.4d;

            return value;
        }
    }
}
