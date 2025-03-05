
namespace StudentUsos.Extensions;

internal class SizePercentageExtension : IMarkupExtension
{
    public enum TypeOptions
    {
        Width,
        Height,
    }
    public TypeOptions Type { get; set; }
    /// <summary>
    /// Value between 0 and 1
    /// </summary>
    public float Percentage { get; set; } = 1;
    public object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Percentage < 0 || Percentage > 1) throw new Exception("Percentage must be between 0 and 1");
        var service = serviceProvider.GetService<IProvideValueTarget>();
        if (service == null)
        {
            return 0;
        }
        if (service.TargetObject is VisualElement visualElement)
        {
            double size;
            if (Type == TypeOptions.Width)
            {
                size = visualElement.Width > 0 ? visualElement.Width : visualElement.WidthRequest;
            }
            else
            {
                size = visualElement.Height > 0 ? visualElement.Height : visualElement.HeightRequest;
            }
            return size * Percentage;
        }
        return 0;
    }
}