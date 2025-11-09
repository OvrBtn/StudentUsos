using StudentUsos.Controls;
using StudentUsos.Views.WhatsNew;

namespace StudentUsos.Features.Settings.Views.Subpages;

public partial class ApplicationSubpage : CustomContentPageNotAnimated
{
    INavigationService navigationService;
    ILocalStorageManager localStorageManager;
    IBackgroundJobService backgroundJobService;
    public ApplicationSubpage(ApplicationSubpageViewModel applicationSubpageViewModel,
        INavigationService navigationService,
        ILocalStorageManager localStorageManager,
        IBackgroundJobService backgroundJobService)
    {
        InitializeComponent();
        BindingContext = applicationSubpageViewModel;
        this.navigationService = navigationService;
        this.localStorageManager = localStorageManager;
        this.backgroundJobService = backgroundJobService;

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
}