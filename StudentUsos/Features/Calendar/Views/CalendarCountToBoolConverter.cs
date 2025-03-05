using System.Globalization;

namespace StudentUsos.Features.Calendar.Views;

public class CalendarCountToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        try
        {
            if (value is null || parameter is null)
            {
                return true;
            }
            int count = int.Parse(value.ToString()!);
            int limit = int.Parse(parameter.ToString()!);
            return count <= limit;
        }
        catch { return true; }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}