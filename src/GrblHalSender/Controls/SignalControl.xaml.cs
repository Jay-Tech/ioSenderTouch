
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GrblHalSender.Controls
{
    public partial class SignalControl : UserControl
    {
        static Brush LEDOn = Brushes.Red, LEDOff = Brushes.LightGray;

        public SignalControl()
        {
            InitializeComponent();

            LEDOff = btnLED.Background;
        }

        public static readonly DependencyProperty IsSetProperty = DependencyProperty.Register(nameof(IsSet), typeof(bool), typeof(SignalControl), new PropertyMetadata(false, new PropertyChangedCallback(OnIsSetChanged)));
        public bool IsSet
        {
            get { return (bool)GetValue(IsSetProperty); }
            set { SetValue(IsSetProperty, value); }
        }
        private static void OnIsSetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SignalControl)d).btnLED.Background = (bool)e.NewValue ? LEDOn : LEDOff;
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(SignalControl), new PropertyMetadata());
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
    }
}

