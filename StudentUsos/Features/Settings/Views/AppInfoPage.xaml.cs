using StudentUsos.Controls;
using StudentUsos.Resources.LocalizedStrings;

namespace StudentUsos.Features.Settings.Views;

public partial class AppInfoPage : CustomContentPageNotAnimated, INavigableWithParameter<bool>
{
    IApplicationService applicationService;
    public AppInfoPage(IApplicationService applicationService)
    {
        InitializeComponent();

        this.applicationService = applicationService;
    }

    public void OnNavigated(bool navigationParameter)
    {
        this.IsTabBarVisible = navigationParameter;
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