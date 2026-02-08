using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GrblHalSender.Convertors
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string parameterString && value != null)
            {
                if (Enum.IsDefined(value.GetType(), value))
                {
                    object parameterValue = Enum.Parse(value.GetType(), parameterString);
                    return value.Equals(parameterValue);
                }
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing; // Not needed for IsEnabled binding
        }
    }
}
