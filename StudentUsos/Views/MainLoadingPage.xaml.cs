using StudentUsos.Controls;

namespace StudentUsos;

public partial class MainLoadingPage : CustomContentPageNotAnimated
{
    public MainLoadingPage()
    {
        InitializeComponent();
    }
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        Dispatcher.Dispatch(() =>
        {
            Shell.Current.GoToAsync("//DashboardPage");
        });
    }
}