using StudentUsos.Controls;
using StudentUsos.Resources.LocalizedStrings;

namespace StudentUsos.Features.Settings.Views;

public partial class SettingsPage : CustomContentPageNotAnimated
{
    public SettingsPage(SettingsViewModel settingsViewModel,
        INavigationService navigationService,
        ILogger? logger = null)
    {
        BindingContext = settingsViewModel;
        InitializeComponent();

        versionLabel.Text = LocalizedStrings.Version + " " + AppInfo.Current.VersionString;
    }
}