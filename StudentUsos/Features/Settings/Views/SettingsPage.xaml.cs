﻿using StudentUsos.Controls;
using StudentUsos.Resources.LocalizedStrings;

namespace StudentUsos.Features.Settings.Views;

public partial class SettingsPage : CustomContentPageNotAnimated
{
    SettingsViewModel settingsViewModel;
    INavigationService navigationService;
    ILogger? logger;
    public SettingsPage(SettingsViewModel settingsViewModel,
        INavigationService navigationService,
        ILogger? logger = null)
    {
        this.navigationService = navigationService;
        this.logger = logger;

        BindingContext = this.settingsViewModel = settingsViewModel;
        InitializeComponent();

        versionLabel.Text = LocalizedStrings.Version + " " + AppInfo.Current.VersionString;
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