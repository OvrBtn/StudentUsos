using System.Globalization;

namespace StudentUsos.Converters;

public class GetPercentageOfValueConverter : IValueConverter
{
    /// <summary>
    /// Get percentage (parameter argument) of value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter">Must be between 0 and 100</param>
    /// <param name="culture"></param>
    /// <returns>parameter/100*value</returns>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is null || parameter is null)
            {
                return 1;
            }
            var val = float.Parse(value.ToString()!);
            var par = float.Parse(parameter.ToString()!);
            return par / 100 * val;
        }
        catch
        {
            return 1;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}