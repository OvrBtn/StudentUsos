using System.Globalization;

namespace StudentUsos.Converters;

public class StringBoolToBoolNegatedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is null)
            {
                return true;
            }
            if (bool.TryParse(value.ToString(), out var parsed)) return !parsed;
            string st = value.ToString()!;
            if (st.ToLower().StartsWith('y') || st.ToLower().StartsWith('t')) return false;
            return true;
        }
        catch
        {
            return true;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}