using System.Globalization;

namespace StudentUsos.Converters
{
    public class EmptyStringToStringConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is null || (string)(value) == string.Empty)
                {
                    if (parameter is null)
                    {
                        return string.Empty;
                    }
                    return parameter;
                }
                return value;
            }
            catch
            {
                return value!;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
