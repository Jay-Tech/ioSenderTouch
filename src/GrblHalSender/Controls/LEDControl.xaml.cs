
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GrblHalSender.Controls
{
    /// <summary>
    /// Interaction logic for LEDControl.xaml
    /// </summary>
    public partial class LEDControl : UserControl
    {
        static Brush LEDOn = Brushes.Red, LEDOff = Brushes.LightGray;

        public LEDControl()
        {
            InitializeComponent();

            LEDOff = btnLED.Background;
        }

        public static readonly DependencyProperty IsSetProperty = DependencyProperty.Register(nameof(IsSet), typeof(bool), typeof(LEDControl), new PropertyMetadata(false, new PropertyChangedCallback(OnIsSetChanged)));
        public bool IsSet
        {
            get { return (bool)GetValue(IsSetProperty); }
            set { SetValue(IsSetProperty, value); }
        }
        private static void OnIsSetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as LEDControl).btnLED.Background = (bool)e.NewValue ? LEDOn : LEDOff;
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(LEDControl), new PropertyMetadata());
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
    }
}
