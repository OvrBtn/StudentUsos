using System.Globalization;

namespace StudentUsos.Converters
{
    public class ClassTypeToColorConverter : IValueConverter
    {
        static Color lectureColor = Utilities.GetColorFromResources("LecturePrimaryColor");
        static Color laboratoryColor = Utilities.GetColorFromResources("LaboratoryPrimaryColor");
        static Color classesColor = Utilities.GetColorFromResources("ClassesPrimaryColor");
        static Color defaultColor = Utilities.GetColorFromResources("Primary");
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            try
            {
                if (value is null)
                {
                    return Colors.Gray;
                }
                string classTypeString = value.ToString()!.ToLower();
                if (classTypeString.Contains("lecture"))
                {
                    return lectureColor;
                }
                else if (classTypeString.Contains("laboratory"))
                {
                    return laboratoryColor;
                }
                else if (classTypeString.Contains("classes"))
                {
                    return classesColor;
                }
                return defaultColor;

            }
            catch (Exception ex)
            {
                Utilities.ShowError(ex);
                return defaultColor;
            }
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
