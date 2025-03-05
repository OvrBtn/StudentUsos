using StudentUsos.Controls;
using StudentUsos.Features.Authorization.Services;
using StudentUsos.Resources.LocalizedStrings;

namespace StudentUsos.Features.Settings.Views;

public partial class SettingsPage : CustomContentPageNotAnimated
{
    SettingsViewModel settingsViewModel;
    INavigationService navigationService;
    ILogger? logger;
    public SettingsPage(SettingsViewModel settingsViewModel,
        INavigationService navigationService,
        ILocalStorageManager localStorageManager,
        ILogger? logger = null)
    {
        this.navigationService = navigationService;
        this.logger = logger;

        BindingContext = this.settingsViewModel = settingsViewModel;
        InitializeComponent();

        this.Disappearing += settingsViewModel.SettingsPage_Disappearing;

        versionLabel.Text = LocalizedStrings.Version + " " + AppInfo.Current.VersionString;
    }

    private void RevokeAccessButton_Clicked(object sender, EventArgs e)
    {
        MessagePopupPage.CreateAndShow(LocalizedStrings.SettingsPage_RevokeAccessTitle, LocalizedStrings.SettingsPage_RevokeAccessDescription,
            LocalizedStrings.Continue, LocalizedStrings.Cancel, ContinueRevokingAccessAsync, () => { });

        async void ContinueRevokingAccessAsync()
        {
            await Browser.OpenAsync("https://usosapps.put.poznan.pl/", BrowserLaunchMode.SystemPreferred);
        }
    }

    private void LogOutButton_Clicked_1(object sender, EventArgs e)
    {
        MessagePopupPage.CreateAndShow(LocalizedStrings.LoggingOut, LocalizedStrings.LoggingOut_Confirmation, LocalizedStrings.Yes, LocalizedStrings.No, () => AuthorizationService.LogoutAsync(), null);
    }

    private void OpenLogsButton_Clicked(object sender, EventArgs e)
    {
        navigationService.PushAsync<LogsPage>();
    }

    private void OpenLogsInfoButton_Clicked(object sender, EventArgs e)
    {
        List<MultipleChoicePopup.Item> options = new()
        {
            new(LoggingPermission.User, LocalizedStrings.LoggingPermissionDescription_User, logger.IsModuleAllowed(LoggingPermission.User)),
            new(LoggingPermission.Progs, LocalizedStrings.LoggingPermissionDescription_Progs, logger.IsModuleAllowed(LoggingPermission.Progs)),
            new(LoggingPermission.Activities, LocalizedStrings.LoggingPermissionDescription_Activities, logger.IsModuleAllowed(LoggingPermission.Activities)),
            new(LoggingPermission.Calendar, LocalizedStrings.LoggingPermissionDescription_Calendar, logger.IsModuleAllowed(LoggingPermission.Calendar)),
            new(LoggingPermission.FinalGrades, LocalizedStrings.LoggingPermissionDescription_FinalGrades, logger.IsModuleAllowed(LoggingPermission.FinalGrades)),
            new(LoggingPermission.Groups, LocalizedStrings.LoggingPermissionDescription_Groups, logger.IsModuleAllowed(LoggingPermission.Groups)),
            new(LoggingPermission.Surveys, LocalizedStrings.LoggingPermissionDescription_Surveys, logger.IsModuleAllowed(LoggingPermission.Surveys)),
            new(LoggingPermission.Payments, LocalizedStrings.LoggingPermissionDescription_Payments, logger.IsModuleAllowed(LoggingPermission.Payments)),
        };
        var multipleChoicePopup = MultipleChoicePopup.CreateAndShow(LocalizedStrings.LoggingPermissionPopup_Title, options);
        multipleChoicePopup.OnConfirmed += MultipleChoicePopup_OnConfirmed;
    }

    private void MultipleChoicePopup_OnConfirmed(List<MultipleChoicePopup.Item> result)
    {
        List<string> ids = new();
        foreach (var item in result)
        {
            if (item.IsChecked) ids.Add(item.Id);
        }
        logger.SetAllowedModules(ids);
    }

    private async void AppInfoButton_Clicked(object sender, EventArgs e)
    {
        await navigationService.PushAsync<AppInfoPage>();
    }
}