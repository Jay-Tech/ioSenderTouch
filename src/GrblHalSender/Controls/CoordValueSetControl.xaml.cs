
using System.Windows;
using System.Windows.Controls;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for CoordValueSetControl.xaml
    /// </summary>
    public partial class CoordValueSetControl : UserControl
    {
        public delegate void ClickHandler(object sender, RoutedEventArgs e);
        public event ClickHandler Click;

        public CoordValueSetControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(CoordValueSetControl), new PropertyMetadata(double.NaN));
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(CoordValueSetControl), new PropertyMetadata());
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public string Text { get { return cvValue.Text; } }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }
    }
}
