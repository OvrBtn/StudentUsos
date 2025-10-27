namespace StudentUsos.Services;

class NavigationService : INavigationService
{

    public static INavigationService Default { get; private set; }

    IServiceProvider _serviceProvider;
    INavigation navigation;
    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        this.navigation = App.Current?.Windows[0]?.Navigation!;

        Default = this;
    }

    public async Task SetAsRootPageAsync(string location, bool isAnimated = true)
    {
        await Shell.Current.GoToAsync(location, isAnimated);
    }

    public async Task<T> PushAsync<T>(bool isAnimated = true) where T : ContentPage
    {
        var page = _serviceProvider.GetService<T>();
        if (page is null)
        {
            throw new ArgumentException("Page not injected into dependency container");
        }
        await navigation.PushAsync(page, isAnimated);
        return page;
    }

    public async Task<T> PushModalAsync<T>(bool isAnimated = true) where T : ContentPage
    {
        var page = _serviceProvider.GetService<T>();
        if (page is null)
        {
            throw new ArgumentException("Page not injected into dependency container");
        }
        await navigation.PushModalAsync(page, isAnimated);
        return page;
    }

    public Task PopAsync(bool isAnimated = true)
    {
        return navigation.PopAsync(isAnimated);
    }

    public Task PopModalAsync(bool isAnimated = true)
    {
        return navigation.PopModalAsync(isAnimated);
    }

    public Task PopToRootAsync(bool isAnimated = true)
    {
        return navigation.PopToRootAsync(isAnimated);
    }
}