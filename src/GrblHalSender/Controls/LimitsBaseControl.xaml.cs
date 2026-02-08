
using System.Windows;
using System.Windows.Controls;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for LimitsBaseControl.xaml
    /// </summary>
    public partial class LimitsBaseControl : UserControl
    {
        public LimitsBaseControl()
        {
            InitializeComponent();

            //UnitProperty.OverrideMetadata(typeof(string), new PropertyMetadata("mm"));
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(LimitsBaseControl), new PropertyMetadata());
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public static readonly DependencyProperty UnitProperty = DependencyProperty.Register(nameof(Unit), typeof(string), typeof(LimitsBaseControl), new PropertyMetadata("mm"));
        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }

        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(LimitsBaseControl), new PropertyMetadata());
        public double MinValue
        {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(LimitsBaseControl), new PropertyMetadata());
        public double MaxValue
        {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }
    }
}
