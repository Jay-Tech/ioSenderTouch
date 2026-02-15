
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GrblHalSender.Controls.Probing
{
    public static class Converters
    {
        public static EnumValueToVisibleConverter EnumValueToVisibleConverter = new EnumValueToVisibleConverter();
        public static OriginToBooleanConverter OriginToBooleanConverter = new OriginToBooleanConverter();
        public static OriginToCurrentPositionConverter OriginToCurrentPositionConverter = new OriginToCurrentPositionConverter();
    }

    public class EnumValueToVisibleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || parameter == null)
                return Visibility.Hidden;

            return values[0].ToString().Equals(parameter.ToString(), StringComparison.InvariantCultureIgnoreCase) &&
                   (values.Length == 2 ? values[1] is bool && (bool)values[1] : true) ? Visibility.Visible : Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class OriginToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is OriginControl.Origin && (OriginControl.Origin)value == OriginControl.Origin.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? OriginControl.Origin.None : OriginControl.Origin.Center;
        }
    }

    public class OriginToCurrentPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is OriginControl.Origin && (OriginControl.Origin)value == OriginControl.Origin.CurrentPos;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? OriginControl.Origin.CurrentPos : OriginControl.Origin.None;
        }
    }
}
