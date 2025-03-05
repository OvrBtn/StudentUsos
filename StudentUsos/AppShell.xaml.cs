namespace StudentUsos;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    protected override void OnNavigating(ShellNavigatingEventArgs args)
    {
        base.OnNavigating(args);

        if (args.Source == ShellNavigationSource.ShellSectionChanged)
        {
            ClearNavigationStack();
        }
    }

    public static void ClearNavigationStack()
    {
        var navigation = Current.Navigation;
        var pages = navigation.NavigationStack;
        for (var i = pages.Count - 1; i >= 1; i--)
        {
            navigation.RemovePage(pages[i]);
        }
    }
}