using StudentUsos.Features.Calendar.Models;
using System.Collections.ObjectModel;
using System.Globalization;

namespace StudentUsos.Converters
{
    public class ContainsElementWithIndexReturnBackgroundColor : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is not ObservableCollection<EventIndicator> collection || parameter is null)
                {
                    return Colors.Black;
                }
                var param = int.Parse(parameter.ToString()!);
                if (collection.Count - 1 >= param) return collection[param].Color;
                return Colors.Transparent;
            }
            catch { return Colors.Black; }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
