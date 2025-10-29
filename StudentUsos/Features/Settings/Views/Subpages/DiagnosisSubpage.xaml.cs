using StudentUsos.Controls;
using StudentUsos.Resources.LocalizedStrings;

namespace StudentUsos.Features.Settings.Views.Subpages;

public partial class DiagnosisSubpage : CustomContentPageNotAnimated
{
    INavigationService navigationService;
    ILogger logger;
    IApplicationService? applicationService;
    public DiagnosisSubpage(INavigationService navigationService,
        ILogger logger,
        IApplicationService? applicationService = null)
    {
        InitializeComponent();

        this.navigationService = navigationService;
        this.logger = logger;
        this.applicationService = applicationService;
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
        logger?.SetAllowedModules(ids);
    }

    public static event Action OnLocalDataReseted;
    static int resetCounter;

    private void ResetLocalDataButton_Clicked(object sender, EventArgs e)
    {
        if (resetCounter >= 2)
        {
            applicationService?.ShowToast(LocalizedStrings.OperationCanceledDueToSpam);
            return;
        }

        resetCounter++;
        BackwardCompatibility.ResetLocalData();
        OnLocalDataReseted?.Invoke();
        applicationService?.ShowToast(LocalizedStrings.Success);
    }
}