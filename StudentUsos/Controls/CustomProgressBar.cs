using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace StudentUsos.Controls;

public class CustomProgressBar : SKCanvasView
{
    public static readonly BindableProperty ProgressColorProperty =
        BindableProperty.Create(nameof(ProgressColor), typeof(Color), typeof(CustomProgressBar), Colors.Blue, BindingMode.Default, null, ProgressBarPropertyChanged);

    public Color ProgressColor
    {
        get => (Color)GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
    }

    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(float), typeof(CustomProgressBar), 0f, BindingMode.TwoWay, null, ProgressBarPropertyChanged);
    /// <summary>
    /// Value between 0 and 1
    /// </summary>
    public float Progress
    {
        get => (float)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public enum DirectionOptions
    {
        LeftToRight,
        RightToLeft,
        BottomToTop,
        TopToBottom,
    }

    public static readonly BindableProperty DirectionProperty =
        BindableProperty.Create(nameof(Direction), typeof(DirectionOptions), typeof(CustomProgressBar), DirectionOptions.LeftToRight, BindingMode.Default, null, ProgressBarPropertyChanged);
    public DirectionOptions Direction
    {
        get => (DirectionOptions)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    private static void ProgressBarPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        ((CustomProgressBar)bindable).InvalidateSurface();
    }

    public CustomProgressBar()
    {

    }

    SKCanvas canvas;
    SKImageInfo info;

    protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        base.OnPaintSurface(e);

        canvas = e.Surface.Canvas;
        canvas.Clear();
        info = e.Info;

        DrawRect();
    }

    void DrawRect()
    {
        SKPaint paint = new()
        {
            Color = ProgressColor.ToSKColor(),
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
        };
        SKRect rect;
        switch (Direction)
        {
            case DirectionOptions.LeftToRight:
                rect = new(0, 0, info.Width * Progress, info.Height);
                break;
            case DirectionOptions.RightToLeft:
                rect = new(info.Width - info.Width * Progress, 0, info.Width, info.Height);
                break;
            case DirectionOptions.TopToBottom:
                rect = new(0, 0, info.Width, info.Height * Progress);
                break;
            case DirectionOptions.BottomToTop:
                rect = new(0, info.Height - info.Height * Progress, info.Width, info.Height);
                break;
            default:
                rect = new(0, 0, info.Width * Progress, info.Height);
                break;


        }
        canvas.DrawRect(rect, paint);
    }

    int animationCounter = 0;
    public void ProgressTo(float progress, uint length, Easing easing)
    {
        float startValue = Progress;
        Animation animation = new((callbackValue) =>
        {
            float lerped = Utilities.Lerp((float)callbackValue, startValue, progress);
            Progress = lerped;
            InvalidateSurface();
        });
        animation.Commit(this, "CustomProgressBarAnimation" + (animationCounter++).ToString(), 16, length, easing);
    }
}