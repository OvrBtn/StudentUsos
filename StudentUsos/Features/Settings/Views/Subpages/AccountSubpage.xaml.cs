using StudentUsos.Controls;
using StudentUsos.Features.Authorization.Services;
using StudentUsos.Resources.LocalizedStrings;

namespace StudentUsos.Features.Settings.Views.Subpages;

public partial class AccountSubpage : CustomContentPageNotAnimated
{
    IUsosInstallationsService usosInstallationsService;
    public AccountSubpage(IUsosInstallationsService usosInstallationsService)
    {
        InitializeComponent();

        this.usosInstallationsService = usosInstallationsService;
    }

    private void RevokeAccessButton_Clicked(object sender, EventArgs e)
    {
        MessagePopupPage.CreateAndShow(LocalizedStrings.SettingsPage_RevokeAccessTitle, LocalizedStrings.SettingsPage_RevokeAccessDescription,
            LocalizedStrings.Continue, LocalizedStrings.Cancel, ContinueRevokingAccessAsync, () => { });

        async void ContinueRevokingAccessAsync()
        {
            await Browser.OpenAsync(usosInstallationsService.GetCurrentInstallation()!, BrowserLaunchMode.SystemPreferred);
        }
    }

    private void LogOutButton_Clicked_1(object sender, EventArgs e)
    {
        MessagePopupPage.CreateAndShow(LocalizedStrings.LoggingOut,
            LocalizedStrings.LoggingOut_Confirmation,
            LocalizedStrings.Yes,
            LocalizedStrings.No,
            () => AuthorizationService.LogoutAsync(),
            null);
    }
}