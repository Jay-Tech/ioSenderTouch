

using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GrblHalSender.GrblCore;

namespace GrblHalSender.Controls
{
    public class NumericComboBox : ComboBox
    {
        private NumericProperties np = new NumericProperties();

        public NumericComboBox()
        {
            IsEditable = true;
        }

        public double Value
        {
            get
            {
                double value = 0.0d;
                double.TryParse(Text, np.Styles, CultureInfo.InvariantCulture, out value);
                return value;
            }
            set
            {
                Text = Math.Round(value, np.Precision).ToString(np.DisplayFormat, CultureInfo.InvariantCulture);
            }
        }

        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register(nameof(Format), typeof(string), typeof(NumericComboBox), new PropertyMetadata(GrblConstants.FORMAT_METRIC, new PropertyChangedCallback(OnFormatChanged)));
        public string Format
        {
            get { return (string)GetValue(FormatProperty); }
            set { SetValue(FormatProperty, value); }
        }
        private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumericProperties.OnFormatChanged(d, ((NumericComboBox)d).np, (string)e.NewValue);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)e.OriginalSource;
            string text = textBox.SelectionLength > 0 ? textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength) : textBox.Text;
            text = text.Insert(textBox.CaretIndex, e.Text);
            e.Handled = !NumericProperties.IsStringNumeric(text, np);
            base.OnPreviewTextInput(e);
        }
    }
}
