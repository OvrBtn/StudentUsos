namespace StudentUsos.Extensions;

public class ScreenSizeExtension : IMarkupExtension
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
    public double DeviceWidth { get => DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density; }
    public double DeviceHeight { get => DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density; }
    public object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Percentage < 0 || Percentage > 1) throw new Exception("Percentage must be between 0 and 1");
        if (Type == TypeOptions.Width) return Percentage * DeviceWidth;
        return Percentage * DeviceHeight;
    }
}