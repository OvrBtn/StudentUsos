using System.Globalization;

namespace StudentUsos.Converters;

public class MergeDateTimesConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is null || parameter is null)
            {
                return string.Empty;
            }
            DateTime firstDate = (DateTime)value;
            DateTime secondDate = (DateTime)parameter;
            return Utilities.MergeDateTimes(firstDate, secondDate);
        }
        catch
        {
            return string.Empty;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}