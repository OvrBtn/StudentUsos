using System.Globalization;

namespace StudentUsos.Converters;

public class ContainsElementWithIndex : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is null || parameter is null)
            {
                return false;
            }
            if (value is not IEnumerable<object> enumerable)
            {
                return false;
            }
            int param = int.Parse(parameter.ToString()!);
            if (enumerable.Count() - 1 >= param) return true;
            else return false;
        }
        catch { return false; }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}