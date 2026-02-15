

using System.Windows;
using System.Windows.Controls;
using GrblHalSender.GrblCore;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for NumericField.xaml
    /// </summary>
    public partial class NumericField : UserControl
    {   

        protected string format;
        protected bool metric = true, allow_dp = true, allow_sign = false;
        protected int precision = 3;

        public NumericField()
        {
            InitializeComponent();
            data.DataContext = this;
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumericField), new PropertyMetadata(double.NaN, new PropertyChangedCallback(OnValueChanged)), new ValidateValueCallback(IsValidReading));
        public double Value
        {
            get { double v = (double)GetValue(ValueProperty); return double.IsNaN(v) ? 0d : v; }
            set { SetValue(ValueProperty, value); }
        }
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (double.IsNaN((double)e.NewValue))
                ((NumericField)d).data.Clear();
        }

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(nameof(Format), typeof(string), typeof(NumericField), new PropertyMetadata(NumericProperties.MetricFormat));
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(NumericField), new PropertyMetadata("Label:"));
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(nameof(Unit), typeof(string), typeof(NumericField), new PropertyMetadata("mm"));
        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public static readonly DependencyProperty Tooltip2Property = DependencyProperty.Register(nameof(Tooltip2), typeof(string), typeof(NumericField), new PropertyMetadata(string.Empty));
        public string Tooltip2
        {
            get { return (string)GetValue(Tooltip2Property); }
            set { SetValue(Tooltip2Property, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(NumericField), new PropertyMetadata());
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public static readonly DependencyProperty ColonAtProperty = DependencyProperty.Register(nameof(ColonAt), typeof(double), typeof(NumericField), new PropertyMetadata(70.0d, new PropertyChangedCallback(OnColonAtChanged)));


        public double ColonAt
        {
            get { return (double)GetValue(ColonAtProperty); }
            set { SetValue(ColonAtProperty, value); }
        }
        private static void OnColonAtChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((NumericField)d).OnColonAtChanged();
        }
        private void OnColonAtChanged()
        {
            grid.ColumnDefinitions[0].Width = new GridLength(ColonAt);
        }

        public string Text
        {
            get { return Value.ToInvariantString(data.DisplayFormat); }
        }

        public static bool IsValidReading(object value)
        {
            double v = (double)value;
            return (!v.Equals(double.PositiveInfinity));
        }

        public Control Field { get { return data; } }

        public void Clear()
        {
            data.Clear();
        }

      
    }
}

