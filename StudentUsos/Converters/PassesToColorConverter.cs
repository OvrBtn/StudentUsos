using System.Globalization;
using StudentUsos.Features.Grades.Models;

namespace StudentUsos.Converters
{
    public class PassesToColorConverter : IValueConverter
    {
        static readonly Color BackgroundColor = Colors.Transparent;
        static readonly Color PassesColor = Utilities.GetColorFromResources("FinalGrade1");
        static readonly Color FailsColor = Utilities.GetColorFromResources("FinalGrade4");
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return BackgroundColor;
                if (value is FinalGrade)
                {
                    FinalGrade finalGrade = (FinalGrade)value;
                    if (finalGrade.IsEmpty) return Colors.White;
                    if (finalGrade.Passes) return PassesColor;
                    return FailsColor;
                }
                var parsed = (FinalGradeGroup)value;
                if (parsed.FirstTermGrade.Passes || parsed.SecondTermGrade.Passes) return PassesColor;
                if (string.IsNullOrEmpty(parsed.FirstTermGrade.ExamId) == false && parsed.FirstTermGrade.Passes == false && parsed.SecondTermGrade.Passes == false) return FailsColor;
                return BackgroundColor;
            }
            catch
            {
                return BackgroundColor;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
