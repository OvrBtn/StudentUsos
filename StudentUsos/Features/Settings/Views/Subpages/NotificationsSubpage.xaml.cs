using StudentUsos.Controls;
using StudentUsos.Features.Settings.Views.NotificationsDiagnosis;

namespace StudentUsos.Features.Settings.Views.Subpages;

public partial class NotificationsSubpage : CustomContentPageNotAnimated
{
    NotificationsSubpageViewModel viewModel;
    INavigationService navigationService;
    ILocalStorageManager localStorageManager;
    public NotificationsSubpage(NotificationsSubpageViewModel viewModel, INavigationService navigationService, ILocalStorageManager localStorageManager)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;

        this.Disappearing += viewModel.SettingsPage_Disappearing;

        this.navigationService = navigationService;
        this.localStorageManager = localStorageManager;

        activitiesBackgroundSyncShouldSendNotifications = localStorageManager.GetBool(LocalStorageKeys.ActivitiesSynchronizationBackgroundWorker_ShouldSendNotifications, true);
        activitiesBackgroundSyncShouldSendNotificationsPrevious = activitiesBackgroundSyncShouldSendNotifications;
        ActivitiesBackgroundSyncNotificationsSwitch.IsToggled = activitiesBackgroundSyncShouldSendNotifications;
    }

    bool activitiesBackgroundSyncShouldSendNotifications, activitiesBackgroundSyncShouldSendNotificationsPrevious;

    bool isViewModelSet = false;
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (isViewModelSet)
        {
            return;
        }
        Dispatcher.Dispatch(() =>
        {
            isViewModelSet = true;
            viewModel.Init();
        });
    }

    private void TroubleshootingButton_Clicked(object sender, EventArgs e)
    {
        navigationService.PushAsync<NotificationsDiagnosisPage>();
    }

    private void ActivitiesBackgroundSyncNotificationsSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        activitiesBackgroundSyncShouldSendNotifications = e.Value;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        if(activitiesBackgroundSyncShouldSendNotificationsPrevious != activitiesBackgroundSyncShouldSendNotifications)
        {
            localStorageManager.SetBool(LocalStorageKeys.ActivitiesSynchronizationBackgroundWorker_ShouldSendNotifications, activitiesBackgroundSyncShouldSendNotifications);
        }
    }
}