namespace StudentUsos.Views;

public class PopupBase : ContentPage
{
    public PopupBase()
    {
        Shell.SetPresentationMode(this, PresentationMode.ModalNotAnimated);
        this.BackgroundColor = Colors.Transparent;
    }

    private void TapGestureRecognizer_Tapped(object? sender, TappedEventArgs e)
    {
        Close();
    }

    Color defaultBackgroundColor = Utilities.GetColorFromResources("BackgroundColor");

    protected override void OnAppearing()
    {
        Color color = new Color(13, 13, 13);

        if (Content != null)
        {
            StackLayout stackLayout = new();
            stackLayout.Children.Add(Content);
            Content = stackLayout;

            Content.SetBinding(OpacityProperty, "ContentOpacity");
            Content.BackgroundColor = new(0, 0, 0, 0.5f);
            Content.VerticalOptions = LayoutOptions.Fill;
            Content.HorizontalOptions = LayoutOptions.Fill;

            TapGestureRecognizer tapGestureRecognizer = new();
            tapGestureRecognizer.Tapped += TapGestureRecognizer_Tapped;
            Content.GestureRecognizers.Add(tapGestureRecognizer);
        }

        base.OnAppearing();
    }

    protected override void OnDisappearing()
    {

        this.Content.Opacity = 0;
        base.OnDisappearing();
    }

    public virtual void Close()
    {
        App.Current?.Windows[0].Page?.Navigation.PopModalAsync(false);
    }
}