using System.Globalization;

namespace StudentUsos.Converters;

public class StringBoolToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is null)
            {
                return false;
            }
            if (bool.TryParse(value.ToString()!, out var parsed)) return parsed;
            string st = value.ToString()!;
            if (st.ToLower().StartsWith('y') || st.ToLower().StartsWith('t')) return true;
            return false;
        }
        catch
        {
            return false;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}