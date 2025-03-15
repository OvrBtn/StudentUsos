using StudentUsos.Controls;
using StudentUsos.Features.Settings.Views.NotificationsDiagnosis;

namespace StudentUsos.Features.Settings.Views.Subpages;

public partial class NotificationsSubpage : CustomContentPageNotAnimated
{
    NotificationsSubpageViewModel viewModel;
    INavigationService navigationService;
    public NotificationsSubpage(NotificationsSubpageViewModel viewModel, INavigationService navigationService)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;

        this.Disappearing += viewModel.SettingsPage_Disappearing;

        this.navigationService = navigationService;
    }

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
}