using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Reflection;

namespace StudentUsos.Controls
{
    public class CustomTabBar : SKCanvasView
    {
        public static readonly BindableProperty ActiveColorProperty =
            BindableProperty.Create(nameof(ActiveTabBackgroundColor), typeof(Color), typeof(CustomProgressBar), Utilities.GetColorFromResources("Primary"), BindingMode.Default, null, BindablePropertyChanged);

        public Color ActiveTabBackgroundColor
        {
            get => (Color)GetValue(ActiveColorProperty);
            set => SetValue(ActiveColorProperty, value);
        }

        private static void BindablePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ((CustomProgressBar)bindable).InvalidateSurface();
        }

        public CustomTabBar()
        {
            this.EnableTouchEvents = true;
            this.Touch += CustomTabBar_Touch;

            Buttons.Add(new("house.png", () => Shell.Current.GoToAsync("//DashboardPage")));
            Buttons.Add(new("timetable.png", () => Shell.Current.GoToAsync("//ActivitiesPage")));
            Buttons.Add(new("grid.png", () => Shell.Current.GoToAsync("//MorePage")));
        }

        private void CustomTabBar_Touch(object? sender, SKTouchEventArgs e)
        {
            e.Handled = true;
            if (e.ActionType != SKTouchAction.Pressed)
            {
                return;
            }
            var location = e.Location;
            for (int i = 0; i < Buttons.Count; i++)
            {
                var button = Buttons[i];
                if (button.SkRect.Contains(location))
                {
                    button.OnClick?.Invoke();
                    ActiveTabIndex = i;
                    InvalidateSurface();
                    break;
                }
            }
        }

        SKCanvas canvas;
        SKImageInfo info;

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            canvas = e.Surface.Canvas;
            canvas.Clear();
            info = e.Info;

            Draw();
        }

        void Draw()
        {
            float currentX = 0;
            float xIteration = info.Width / Buttons.Count;
            for (int i = 0; i < Buttons.Count; i++)
            {
                SKPath path = new();
                SKRect rect = new(currentX, 0, currentX + xIteration, info.Height);
                Buttons[i].SkRect = rect;
                path.AddRect(rect);
                SKPaint paint = new()
                {
                    Color = i == ActiveTabIndex ? ActiveTabBackgroundColor.ToSKColor() : Colors.Transparent.ToSKColor(),
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true,
                };
                canvas.DrawPath(path, paint);
                int imageSize = 60;
                SKRect skRectImage = new SKRect(rect.MidX - imageSize / 2, rect.MidY - imageSize / 2, rect.MidX + imageSize / 2, rect.MidY + imageSize / 2);
                canvas.DrawImage(Buttons[i].SkImage, skRectImage);
                currentX += xIteration;
            }

        }

        public static int ActiveTabIndex { get; set; } = 0;
        public List<TabBarButton> Buttons { get; init; } = new();

        public class TabBarButton
        {
            string ImageName { get; set; }
            public SKImage? SkImage { get; set; }
            public Action OnClick { get; set; }
            public SKRect SkRect { get; set; }
            public TabBarButton(string imageName, Action onClick)
            {
                ImageName = imageName;
                OnClick = onClick;
                GetImage();
            }

            void GetImage()
            {
                SkImage = GetSkImage(ImageName);
            }
        }

        public static SKImage? GetSkImage(string name)
        {
            using var stream = GetImageStreamFromResources(name);
            if (stream is null)
            {
                return null;
            }
            var skData = SKData.Create(stream, stream.Length);
            var skCodec = SKCodec.Create(skData);
            var skBitmap = SKBitmap.Decode(skCodec);
            return SKImage.FromBitmap(skBitmap);
        }

        public static Stream? GetImageStreamFromResources(string name)
        {

            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("Studencki_USOS_PUT_MAUI.Resources.Images.TabBar." + name);
            return stream;
        }
    }
}
