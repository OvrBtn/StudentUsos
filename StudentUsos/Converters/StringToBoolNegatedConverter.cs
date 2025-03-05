using System.Globalization;

namespace StudentUsos.Converters;

public class StringToBoolNegatedConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value == null || (string)(value) == "") return true;
            return false;
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