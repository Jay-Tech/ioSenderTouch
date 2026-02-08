
using System.Windows;
using System.Windows.Controls;

namespace GrblHalSender.Controls
{
    public partial class OriginControl : UserControl
    {
        public enum Origin
        {
            None = 0,
            A,
            B,
            C,
            D,
            Center,
            AB,
            AD,
            CB,
            CD,
            CurrentPos
        }

        // D |-----| C
        //   |  Z  |
        // A | ----| B
        public OriginControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(Origin), typeof(OriginControl), new PropertyMetadata(Origin.None));
        public Origin Value
        {
            get { return (Origin)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }
}
