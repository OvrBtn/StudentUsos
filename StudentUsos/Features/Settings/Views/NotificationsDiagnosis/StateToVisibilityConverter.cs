using System.Globalization;

namespace StudentUsos.Features.Settings.Views.NotificationsDiagnosis
{
    public class StateToVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is States state == false)
            {
                return false;
            }
            return state == States.Error || state == States.Warning;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
