/*
 * DROBaseControl.xaml.cs - part of CNC Controls library
 *
 * v0.03 / 2020-01-27 / Io Engineering (Terje Io)
 *
 */

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GrblHalSender.Controls
{
    public partial class DROBaseControl : UserControl
    {
        private static Brush ScaledOn = Brushes.Yellow, ScaledOff;

        public delegate void ZeroClickHandler(object sender, RoutedEventArgs e);
        public event ZeroClickHandler ZeroClick;

        public event EventHandler OnAxisHomeClick;

        public DROBaseControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(DROBaseControl), new PropertyMetadata());
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
     
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(DROBaseControl), new PropertyMetadata());
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty MPosProperty = DependencyProperty.Register(nameof(MPos), typeof(string), typeof(DROBaseControl), new PropertyMetadata());
        public string MPos
        {
            get => (string)GetValue(MPosProperty);
            set => SetValue(MPosProperty, value);
        }



        public bool IsReadOnly
        {
            get { return txtReadout.IsReadOnly; }
            set { txtReadout.IsReadOnly = value; }
        }

        public static readonly DependencyProperty IsScaledProperty = DependencyProperty.Register(nameof(IsScaled), typeof(bool), typeof(DROBaseControl), new PropertyMetadata(false, new PropertyChangedCallback(OnIsScaledChanged)));
        private string _mPos;

        public bool IsScaled
        {
            get { return (bool)GetValue(IsScaledProperty); }
            set { SetValue(IsScaledProperty, value); }
        }
        private static void OnIsScaledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
           // ((DROBaseControl)d).btnScaled.Background = (bool)e.NewValue ? ScaledOn : ScaledOff;
        }

        public new object Tag
        {
            get { return txtReadout.Tag; }
            set { txtReadout.Tag = btnZero.Tag = value; }
        }
        private void btnHome_Click(object sender, RoutedEventArgs e)
        {
            OnAxisHomeClick?.Invoke(sender, e);
        }
        private void btnZero_Click(object sender, RoutedEventArgs e)
        {
            ZeroClick?.Invoke(sender, e);
        }

    }
}

