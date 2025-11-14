using System.Runtime.CompilerServices;

namespace StudentUsos.Services.Logger;

public interface ILogger
{
    public List<string> GetAllowedModules();
    public bool IsModuleAllowed(LoggingPermission permission);
    public void SetAllowedModules(IEnumerable<string> newAllowedModules);

    public Task<bool> TrySendingLogsToServerAsync();

    public void LogCatchedException(Exception ex,
        [CallerMemberName] string callerName = "",
        [CallerLineNumber] int callerLineNumber = 0);

    public void Log(LogLevel logLevel,
        string message = "",
        [CallerMemberName] string callerName = "",
        [CallerLineNumber] int callerLineNumber = 0);

    public void Log(LogLevel logLevel,
        Exception ex,
        [CallerMemberName] string callerName = "",
        [CallerLineNumber] int callerLineNumber = 0);

    public void Log(LogLevel logLevel,
        string message,
        Exception ex,
        [CallerMemberName] string callerName = "",
        [CallerLineNumber] int callerLineNumber = 0);

}