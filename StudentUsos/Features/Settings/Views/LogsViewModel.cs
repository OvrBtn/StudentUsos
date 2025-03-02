using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StudentUsos.Features.Settings.Views;

public partial class LogsViewModel : BaseViewModel
{
    ILocalDatabaseManager localDatabaseManager;
    public LogsViewModel(ILocalDatabaseManager localDatabaseManager)
    {
        this.localDatabaseManager = localDatabaseManager;
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
}