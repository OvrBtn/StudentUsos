using StudentUsos.Controls;
using StudentUsos.Resources.LocalizedStrings;

namespace StudentUsos.Features.Settings.Views;

public partial class AppInfoPage : CustomContentPageNotAnimated
{
    IApplicationService applicationService;
    public AppInfoPage(IApplicationService applicationService)
    {
        InitializeComponent();

        this.applicationService = applicationService;
    }

    void CopyEmailToClipboard(object sender, EventArgs e)
    {
        Clipboard.Default.SetTextAsync("studenckiusosput@gmail.com");
        applicationService.ShowToast(LocalizedStrings.PersonDetailsPage_EmailCopied);
    }

    private async void DiscordButton_Clicked(object sender, EventArgs e)
    {
        await Browser.OpenAsync("https://discord.gg/h5pASvNG44", BrowserLaunchMode.SystemPreferred);
    }

    private async void GitHubButton_Clicked(object sender, EventArgs e)
    {
        await Browser.OpenAsync("https://github.com/OvrBtn/StudentUsos", BrowserLaunchMode.SystemPreferred);
    }
}