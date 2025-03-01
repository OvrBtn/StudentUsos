using System.Globalization;

namespace StudentUsos.Converters
{
    public class GradeSymbolToColorConverter : IValueConverter
    {
        static readonly Color GradeColor1 = Utilities.GetColorFromResources("FinalGrade1");
        static readonly Color GradeColor2 = Utilities.GetColorFromResources("FinalGrade2");
        static readonly Color GradeColor3 = Utilities.GetColorFromResources("FinalGrade3");
        static readonly Color GradeColor4 = Utilities.GetColorFromResources("FinalGrade4");
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value == null) return Colors.Transparent;
                string grade = value.ToString()!;
                if (grade.Length == 0) return Colors.Transparent;
                if (grade[0] == '5' || grade == "ZAL") return GradeColor1;
                if (grade[0] == '4') return GradeColor2;
                if (grade[0] == '3') return GradeColor3;
                if (grade[0] == '2') return GradeColor4;
                return GradeColor4;
            }
            catch { return Colors.Transparent; }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
