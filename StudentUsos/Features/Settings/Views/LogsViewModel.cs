using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentUsos.Resources.LocalizedStrings;

namespace StudentUsos.Features.Settings.Views;

public partial class LogsViewModel : BaseViewModel
{
    ILogger logger;
    ILocalDatabaseManager localDatabaseManager;
    IApplicationService applicationService;
    public LogsViewModel(ILocalDatabaseManager localDatabaseManager, ILogger logger, IApplicationService applicationService)
    {
        this.localDatabaseManager = localDatabaseManager;
        this.logger = logger;
        this.applicationService = applicationService;
    }

    [ObservableProperty] List<LogRecord> logs;

    public void Init()
    {
        Logs = localDatabaseManager.GetAll<LogRecord>();
    }

    [RelayCommand]
    public void RemoveAllLogs()
    {
        localDatabaseManager.ClearTable<LogRecord>();
        Logs = new();
    }

    [RelayCommand]
    public async Task SendAllLogsAsync()
    {
        bool isSuccess = await logger.TrySendingLogsToServerAsync();
        if (isSuccess == false)
        {
            applicationService.ShowToast(LocalizedStrings.Errors_ConnectionError);
            return;
        }
        RemoveAllLogs();
        applicationService.ShowToast(LocalizedStrings.Success);
    }

    [RelayCommand]
    public async Task LogClickedAsync(LogRecord log)
    {
        await Clipboard.SetTextAsync(log.LogRecordString);
        applicationService.ShowToast(LocalizedStrings.CopiedToClipboard);
    }
}