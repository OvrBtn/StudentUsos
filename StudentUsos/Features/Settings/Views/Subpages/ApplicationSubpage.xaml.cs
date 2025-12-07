using StudentUsos.Controls;
using StudentUsos.Resources.LocalizedStrings;
using StudentUsos.Views.WhatsNew;

namespace StudentUsos.Features.Settings.Views.Subpages;

public partial class ApplicationSubpage : CustomContentPageNotAnimated
{
    INavigationService navigationService;
    ILocalStorageManager localStorageManager;
    IBackgroundJobService backgroundJobService;
    IApplicationService applicationService;
    public ApplicationSubpage(ApplicationSubpageViewModel applicationSubpageViewModel,
        INavigationService navigationService,
        ILocalStorageManager localStorageManager,
        IBackgroundJobService backgroundJobService,
        IApplicationService applicationService)
    {
        InitializeComponent();
        BindingContext = applicationSubpageViewModel;
        this.navigationService = navigationService;
        this.localStorageManager = localStorageManager;
        this.backgroundJobService = backgroundJobService;
        this.applicationService = applicationService;

        isActivitiesBackgroundSyncEnabled = localStorageManager.GetBool(LocalStorageKeys.ActivitiesSynchronizationBackgroundWorker_IsEnabled, true);
        isActivitiesBackgroundSyncEnabledPrevious = isActivitiesBackgroundSyncEnabled;
        ActivitiesBackgroundSyncSwitch.IsToggled = isActivitiesBackgroundSyncEnabled;
    }

    bool isActivitiesBackgroundSyncEnabled = true;
    bool isActivitiesBackgroundSyncEnabledPrevious = true;

    private async void AppInfoButton_Clicked(object sender, EventArgs e)
    {
        await navigationService.PushAsync<AppInfoPage>();
    }

    private async void FeaturedChangesButton_Clicked(object sender, EventArgs e)
    {
        await navigationService.PushModalAsync<WhatsNewCarouselPage>();
    }

    private async void GeneralChangesButton_Clicked(object sender, EventArgs e)
    {
        await navigationService.PushModalAsync<WhatsNewListPage>();
    }

    private void ActivitiesBackgroundSyncSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        isActivitiesBackgroundSyncEnabled = e.Value;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        if (isActivitiesBackgroundSyncEnabledPrevious != isActivitiesBackgroundSyncEnabled)
        {
            backgroundJobService.SetActivitiesBackgroundSynchronizationEnabled(isActivitiesBackgroundSyncEnabled);
            localStorageManager.SetBool(LocalStorageKeys.ActivitiesSynchronizationBackgroundWorker_IsEnabled, isActivitiesBackgroundSyncEnabled);
        }
    }

    void CopyEmailToClipboard(object sender, EventArgs e)
    {
        Clipboard.Default.SetTextAsync("studenckiusosput@gmail.com");
        applicationService.ShowToast(LocalizedStrings.PersonDetailsPage_EmailCopied);
    }

    private async void DiscordButton_Clicked(object sender, EventArgs e)
    {
        await Browser.OpenAsync(LocalizedStrings.Constants_DiscordUrl, BrowserLaunchMode.SystemPreferred);
    }

    private async void GitHubButton_Clicked(object sender, EventArgs e)
    {
        await Browser.OpenAsync(LocalizedStrings.Constants_GitHubUrl, BrowserLaunchMode.SystemPreferred);
    }
}