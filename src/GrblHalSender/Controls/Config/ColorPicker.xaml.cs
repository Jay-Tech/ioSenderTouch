
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GrblHalSender.Controls.Config
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        bool xx = false;

        public ColorPicker()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker), new PropertyMetadata(Colors.AliceBlue, new PropertyChangedCallback(OnIsSelectedColorChanged)));
        public Color SelectedColor
        {
            get { return (Color)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }
        private static void OnIsSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ColorPicker)d).cbut.Background = new SolidColorBrush((Color)e.NewValue);
        }

        public static readonly DependencyProperty IsPickerOpenProperty = DependencyProperty.Register(nameof(IsPickerOpen), typeof(bool), typeof(ColorPicker), new PropertyMetadata(false));
        public bool IsPickerOpen
        {
            get { return (bool)GetValue(IsPickerOpenProperty); }
            set { SetValue(IsPickerOpenProperty, value); }
        }

        private void Popup_Open(object sender, RoutedEventArgs e)
        {
            if ((IsPickerOpen = !IsPickerOpen) && !xx)
            {
                xx = true;
                var ccv = new ColorConverter();
                var colors = (typeof(Colors)).GetProperties();

                const double btnSize = 18d;

                for (var i = 0; i < colors.Length; i++)
                {

//                    if (colors[i].Name == "Transparent")
//                        continue;

                    var color = (Color)ColorConverter.ConvertFromString(colors[i].Name);

                    Button b = new Button
                    {
                        Width = btnSize,
                        Height = btnSize,
                        Focusable = false,
                        Background = new SolidColorBrush(color)
                    };

                    b.Click += Color_Click;

                    popup.Children.Add(b);
                    Canvas.SetTop(b, 14d + Math.Floor((double)(i / 10)) * (btnSize + 2d));
                    Canvas.SetLeft(b, 4d + (i % 10) * (btnSize + 2d));
                }

                popup.Height = 16d + 15d * (btnSize + 2d);
            }
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            SelectedColor = ((SolidColorBrush)(sender as Button).Background).Color;
            IsPickerOpen = false;
        }

        private void Popup_Close(object sender, RoutedEventArgs e)
        {
            IsPickerOpen = false;
        }
    }
}
