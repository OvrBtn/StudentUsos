﻿using StudentUsos.Features.Authorization.Services;
using StudentUsos.Features.UserInfo;
using StudentUsos.Services.ServerConnection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace StudentUsos.Services.Logger;

public class Logger : ILogger
{
    public static ILogger? Default { get; private set; }

    //using lazy to avoid circular dependency
    Lazy<ILocalStorageManager> localStorageManager;
    Lazy<ILocalDatabaseManager> localDatabaseManager;
    Lazy<IServerConnectionManager> serverConnectionManager;
    Lazy<IApplicationService> applicationService;
    Lazy<IUserInfoRepository> userInfoRepository;
    public Logger(
        Lazy<ILocalStorageManager> localStorageManager,
        Lazy<ILocalDatabaseManager> localDatabaseManager,
        Lazy<IServerConnectionManager> serverConnectionManager,
        Lazy<IApplicationService> applicationService,
        Lazy<IUserInfoRepository> userInfoRepository)
    {
        Default = this;

        this.localStorageManager = localStorageManager;
        this.localDatabaseManager = localDatabaseManager;
        this.serverConnectionManager = serverConnectionManager;
        this.applicationService = applicationService;
        this.userInfoRepository = userInfoRepository;

        _ = RunLogCleanupAsync();
    }

    async Task RunLogCleanupAsync()
    {
        await Task.Delay(8000);

        const int deleteLogsAfterDays = 1;
        var deleteThresholdUnixtime = DateTimeOffset.UtcNow.AddDays(deleteLogsAfterDays * -1).ToUnixTimeSeconds();
        localDatabaseManager.Value.Remove<LogRecord>($"CreationDateUnix < {deleteThresholdUnixtime} AND IsSynchronizedWithServer = 1");

        //if unhandled exception happens it might not get sent to server, this makes sure it does
        if (localDatabaseManager.Value.Get<LogRecord>(x => x.LogLevel == LogLevel.Fatal.ToString()) is not null)
        {
            await TrySendingLogsToServerAsync();
        }
    }

    List<string>? allowedModules = null;

    public List<string> GetAllowedModules()
    {
        if (allowedModules is null)
        {
            if (localStorageManager.Value.TryGettingData(LocalStorageKeys.LoggingAllowedData, out string permissions))
            {
                allowedModules = permissions.Split("|").ToList();
            }
            else
            {
                allowedModules = new();
            }
        }
        return new(allowedModules);
    }

    public bool IsModuleAllowed(LoggingPermission permission)
    {
        if (allowedModules is null)
        {
            GetAllowedModules();
        }
        return allowedModules!.Contains(permission.ToString());
    }

    public void SetAllowedModules(IEnumerable<string> newAllowedModules)
    {
        localStorageManager.Value.SetData(LocalStorageKeys.LoggingAllowedData, string.Join("|", newAllowedModules));
        allowedModules = new(newAllowedModules);
    }

    public void LogCatchedException(Exception ex,
        [CallerMemberName] string callerName = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(LogLevel.Error, "Catched exception", ex, callerName, callerLineNumber);

        applicationService.Value.ShowSnackBarAsync($"{callerName}: {callerLineNumber} - {ex.Message}", "ok");
    }

    public void Log(LogLevel logLevel,
        string message = "",
        [CallerMemberName] string callerName = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(logLevel, message, null, callerName, callerLineNumber);
    }

    public void Log(LogLevel logLevel,
        Exception ex,
        [CallerMemberName] string callerName = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(logLevel, string.Empty, ex, callerName, callerLineNumber);
    }

    public void Log(LogLevel logLevel,
        string message,
        Exception ex,
        [CallerMemberName] string callerName = "",
        [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(logLevel, message, ex, callerName, callerLineNumber);
    }

    void LogInternal(LogLevel logLevel,
        string message,
        Exception? ex,
        string callerName = "",
        int callerLineNumber = 0)
    {
        string exceptionMessage = ex?.Message ?? string.Empty;
        string exceptionSerialized = ex?.ToString() ?? string.Empty;

        LogRecord log = new()
        {
            LogLevel = logLevel.ToString(),
            Message = message,
            ExceptionMessage = exceptionMessage,
            ExceptionSerialized = exceptionSerialized,
            CallerName = callerName,
            CallerLineNumber = callerLineNumber.ToString()
        };
        localDatabaseManager.Value.Insert(log);

        if (logLevel >= LogLevel.Error)
        {
            _ = applicationService.Value.WorkerThreadInvoke(TrySendingLogsToServerAsync);
        }
    }


    async Task TrySendingLogsToServerAsync()
    {
        var userInfo = userInfoRepository.Value.GetUserInfo();
        if (userInfo is null)
        {
            return;
        }

        var allLogs = localDatabaseManager.Value.GetAll<LogRecord>();
        if (allLogs.Count == 0)
        {
            return;
        }

        long unixTime;
        if (DateTime.TryParse(allLogs.Last().CreationDate, out var parsed))
        {
            unixTime = new DateTimeOffset(parsed).ToUnixTimeSeconds();
        }
        else
        {
            //add milliseconds to make sure that exception which triggered this operation also gets removed
            unixTime = DateTimeOffset.Now.AddMilliseconds(20).ToUnixTimeSeconds();
        }

        var args = new LogRequestPayload()
        {
            Logs = allLogs,
            Installation = AuthorizationService.Installation,
            UserUsosId = userInfo.Id
        };
        var serialized = JsonSerializer.Serialize(args, LogRequestPayloadJsonContext.Default.LogRequestPayload);

        var result = await serverConnectionManager.Value.SendAuthorizedPostRequestAsync("logs/log", serialized, AuthorizationMode.Full);
        if (result is not null && result.IsSuccess)
        {
            localDatabaseManager.Value.ExecuteQuery($"UPDATE {nameof(LogRecord)} SET {nameof(LogRecord.IsSynchronizedWithServer)} = 1 WHERE {nameof(LogRecord.CreationDateUnix)} <= {unixTime};");
        }
    }
}