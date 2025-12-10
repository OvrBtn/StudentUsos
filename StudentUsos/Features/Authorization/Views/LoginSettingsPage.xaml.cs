using StudentUsos.Controls;
using StudentUsos.Features.Settings.Views;
using StudentUsos.Features.Settings.Views.Subpages;
using StudentUsos.Resources.LocalizedStrings;

namespace StudentUsos.Features.Authorization.Views;

public partial class LoginSettingsPage : CustomContentPageNotAnimated
{
    INavigationService navigationService;
    IApplicationService applicationService;
    public LoginSettingsPage(INavigationService navigationService, IApplicationService applicationService)
    {
        InitializeComponent();
        this.navigationService = navigationService;
        this.applicationService = applicationService;
    }

    private void AppInfoButton_Clicked(object sender, EventArgs e)
    {
        navigationService.PushAsync<AppInfoPage, bool>(false);
    }

    void CopyEmailToClipboard(object sender, EventArgs e)
    {
        Clipboard.Default.SetTextAsync(LocalizedStrings.Constants_Email);
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

    private void LogsButton_Clicked(object sender, EventArgs e)
    {
        navigationService.PushAsync<LogsPage, bool>(false);
    }

    private void DevTunnelButton_Clicked_1(object sender, EventArgs e)
    {
        navigationService.PushAsync<DevTunnelPage, bool>(false);
    }
}