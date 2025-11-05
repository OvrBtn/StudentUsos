using StudentUsos.Features.Authorization.Services;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace StudentUsos.Services.ServerConnection;

public class ServerConnectionManager : IServerConnectionManager
{
    ILogger logger;
    IApplicationService applicationService;
    public ServerConnectionManager(ILogger logger, IApplicationService applicationService)
    {
        this.logger = logger;
        this.applicationService = applicationService;
    }

    string apiVersion = "1";

    const int HttpClientDefaultTimeoutSeconds = 10;
    HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(HttpClientDefaultTimeoutSeconds) };

    const string QueryMethodKey = "QueryMethod";
    const string QueryArgumentsKey = "QueryArguments";
    const string InternalAccessTokenKey = "InternalAccessToken";
    const string UsosAccessTokenKey = "UsosAccessToken";
    const string InternalAccessTokenSecretKey = "InternalAccessTokenSecret";
    const string HashKey = "Hash";
    const string TimestampKey = "Timestamp";
    const string InstallationKey = "Installation";
    const string InternalConsumerKeyKey = "InternalConsumerKey";
    const string InternalConsumerKeySecretKey = "InternalConsumerKeyCecret";
    const string ApiVersionKey = "ApiVersion";
    const string ApplicationVersionKey = "ApplicationVersion";


    public async Task<string?> SendRequestToUsosAsync(string methodName,
        Dictionary<string, string> arguments,
        Action<string>? onRequestFinished = null,
        int timeout = HttpClientDefaultTimeoutSeconds)
    {
        try
        {
            Dictionary<string, string> payload = new();
            Dictionary<string, string> headers = new();
            payload.Add(QueryMethodKey, methodName);
            payload.Add(QueryArgumentsKey, JsonSerializer.Serialize(arguments));
            headers.Add(InternalAccessTokenKey, AuthorizationService.InternalAccessToken);
            headers.Add(UsosAccessTokenKey, AuthorizationService.AccessToken);

            Dictionary<string, string> dataToHash = new() { { InternalAccessTokenSecretKey, AuthorizationService.InternalAccessTokenSecret } };

            var result = await SendGetRequestInternalAsync("usos/query", payload, headers, dataToHash, timeout);
            if (result == null || result.IsSuccess == false)
            {
                return null;
            }
            TryLogging(methodName, result.Response);
            onRequestFinished?.Invoke(result.Response);
            return result.Response;
        }
        catch
        {
            return null;
        }
    }

    public async Task<RequestResult?> SendGetRequestAsync(string endpoint,
        Dictionary<string, string> requestPayload,
        Dictionary<string, string>? additionalDataToHashButNotSend = null,
        int timeout = HttpClientDefaultTimeoutSeconds)
    {
        try
        {
            return await SendGetRequestInternalAsync(endpoint, requestPayload, new(), additionalDataToHashButNotSend, timeout);
        }
        catch
        {
            return null;
        }
    }

    async Task<RequestResult?> SendGetRequestInternalAsync(string endpoint,
        Dictionary<string, string> requestPayload,
        Dictionary<string, string> requestHeaders,
        Dictionary<string, string>? additionalDataToHashButNotSend = null,
        int timeout = HttpClientDefaultTimeoutSeconds)
    {
        try
        {
            string fullPath = Secrets.Default.ServerUrl + "api/v" + apiVersion + "/" + endpoint;

            AddStaticPublicArguments(ref requestHeaders);
            var dictionaryToHash = AddStaticSecretArguments(requestHeaders);
            if (additionalDataToHashButNotSend != null) AddToDictionary(ref dictionaryToHash, additionalDataToHashButNotSend);
            AddToDictionary(ref dictionaryToHash, requestPayload);

            string hash = CreateHash(fullPath, dictionaryToHash);
            requestHeaders.Add(HashKey, hash);

            fullPath = fullPath + "?" + ParseDictionaryArgumentsToUrlFormat(requestPayload);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, fullPath);
            foreach (var item in requestHeaders)
            {
                httpRequestMessage.Headers.Add(item.Key, item.Value);
            }

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
            var response = await httpClient.SendAsync(httpRequestMessage, cancellationTokenSource.Token);
            var result = await response.Content.ReadAsStringAsync();
            return new(response.IsSuccessStatusCode, response, result);
        }
        catch
        {
            return null;
        }
    }

    public async Task<RequestResult?> SendAuthorizedGetRequestAsync(string endpoint,
        Dictionary<string, string> requestPayload,
        AuthorizationMode authorization,
        int timeout = HttpClientDefaultTimeoutSeconds)
    {
        Dictionary<string, string> headers = new();
        Dictionary<string, string> dataToHash = new();
        if (authorization == AuthorizationMode.Full)
        {
            headers[InternalAccessTokenKey] = AuthorizationService.InternalAccessToken;
            headers[UsosAccessTokenKey] = AuthorizationService.AccessToken;
            dataToHash[InternalAccessTokenSecretKey] = AuthorizationService.InternalAccessTokenSecret;
        }
        return await SendGetRequestInternalAsync(endpoint, requestPayload, headers, dataToHash, timeout);
    }

    public async Task<RequestResult?> SendPostRequestAsync(string endpoint,
        string postBody,
        Dictionary<string, string> requestHeaders,
        Dictionary<string, string>? additionalDataToHashButNotSend = null,
        int timeout = HttpClientDefaultTimeoutSeconds)
    {
        try
        {
            string fullPath = Secrets.Default.ServerUrl + "api/v" + apiVersion + "/" + endpoint;

            AddStaticPublicArguments(ref requestHeaders);
            var dictionaryToHash = AddStaticSecretArguments(requestHeaders);
            if (additionalDataToHashButNotSend != null) AddToDictionary(ref dictionaryToHash, additionalDataToHashButNotSend);

            string hash = CreateHash(fullPath, dictionaryToHash, postBody);
            requestHeaders.Add(HashKey, hash);

            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
            using StringContent content = new(postBody, Encoding.UTF8, "application/json");

            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, fullPath)
            {
                Content = content,
            };
            foreach (var item in requestHeaders)
            {
                requestMessage.Headers.Add(item.Key, item.Value);
            }

            //var curl = ConvertToCurlForWindows(requestMessage);


            using var response = await httpClient.SendAsync(requestMessage, cancellationTokenSource.Token);
            var result = await response.Content.ReadAsStringAsync();

            return new(response.IsSuccessStatusCode, response, result);
        }
        catch
        {
            return null;
        }
    }

    public async Task<RequestResult?> SendAuthorizedPostRequestAsync(string endpoint,
        string postBody,
        AuthorizationMode authorization,
        int timeout = HttpClientDefaultTimeoutSeconds)
    {
        Dictionary<string, string> args = new();
        Dictionary<string, string> argsSecret = new();
        if (authorization == AuthorizationMode.Full)
        {
            args[InternalAccessTokenKey] = AuthorizationService.InternalAccessToken;
            args[UsosAccessTokenKey] = AuthorizationService.AccessToken;
            argsSecret[InternalAccessTokenSecretKey] = AuthorizationService.InternalAccessTokenSecret;
        }
        return await SendPostRequestAsync(endpoint, postBody, args, argsSecret, timeout);
    }

    public async Task<RequestResult?> SendAuthorizedPostRequestAsync(string endpoint,
        Dictionary<string, string> requestPayload,
        AuthorizationMode authorization,
        int timeout = HttpClientDefaultTimeoutSeconds)
    {
        string serializedPostBody = JsonSerializer.Serialize(requestPayload, JsonContext.Default.DictionaryStringString);

        Dictionary<string, string> args = new();
        Dictionary<string, string> argsSecret = new();
        if (authorization == AuthorizationMode.Full)
        {
            args[InternalAccessTokenKey] = AuthorizationService.InternalAccessToken;
            args[UsosAccessTokenKey] = AuthorizationService.AccessToken;
            argsSecret[InternalAccessTokenSecretKey] = AuthorizationService.InternalAccessTokenSecret;
        }
        return await SendPostRequestAsync(endpoint, serializedPostBody, args, argsSecret, timeout);
    }

    void AddToDictionary(ref Dictionary<string, string> target, Dictionary<string, string> source)
    {
        foreach (var item in source)
        {
            target[item.Key] = item.Value;
        }
    }

    void AddStaticPublicArguments(ref Dictionary<string, string> args)
    {
        args.Add(TimestampKey, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        args.Add(InstallationKey, AuthorizationService.Installation);
        args.Add(InternalConsumerKeyKey, Secrets.Default.InternalConsumerKey);
        args.Add(ApiVersionKey, apiVersion);
        args.Add(ApplicationVersionKey, applicationService.ApplicationInfo.VersionString);
    }

    Dictionary<string, string> AddStaticSecretArguments(Dictionary<string, string> args)
    {
        Dictionary<string, string> result = new(args)
        {
            { InternalConsumerKeySecretKey, Secrets.Default.InternalConsumerKeySecret }
        };
        return result;
    }

    string ParseDictionaryArgumentsToUrlFormat(Dictionary<string, string> args)
    {
        StringBuilder result = new();
        int counter = 0;
        foreach (var item in args)
        {
            result.Append(item.Key);
            result.Append("=");
            result.Append(item.Value);
            if (counter != args.Count - 1)
            {
                result.Append("&");
            }
            counter++;
        }
        return result.ToString();
    }

    string CreateHash(string fullPath, Dictionary<string, string> arguments, string body = "")
    {
        var argumentsSorted = arguments.OrderBy(x => x.Key).ToDictionary();
        string concateneted = fullPath;
        foreach (var item in argumentsSorted)
        {
            if (item.Key.ToLower() == HashKey)
            {
                continue;
            }
            concateneted += item.Value;
        }
        concateneted += body;
        return HashValue(concateneted);
    }

    string HashValue(string value)
    {
        using HMACSHA256 sha256 = new(Encoding.UTF8.GetBytes(Secrets.Default.InternalConsumerKeySecret));
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower(System.Globalization.CultureInfo.CurrentCulture);
    }

    void TryLogging(string methodName, string response)
    {
        if (logger is null)
        {
            return;
        }

        bool canLog = methodName switch
        {
            _ when methodName.StartsWith("services/tt") => logger.IsModuleAllowed(LoggingPermission.Activities),
            _ when methodName.StartsWith("services/calendar") => logger.IsModuleAllowed(LoggingPermission.Calendar),
            _ when methodName.StartsWith("services/grades") => logger.IsModuleAllowed(LoggingPermission.FinalGrades),
            _ when methodName.StartsWith("services/groups") => logger.IsModuleAllowed(LoggingPermission.Groups),
            _ when methodName.StartsWith("services/payments") => logger.IsModuleAllowed(LoggingPermission.Payments),
            _ when methodName.StartsWith("services/surveys") => logger.IsModuleAllowed(LoggingPermission.Surveys),
            _ when methodName.StartsWith("services/users") => logger.IsModuleAllowed(LoggingPermission.User),
            _ when methodName.StartsWith("services/progs/student") => logger.IsModuleAllowed(LoggingPermission.Progs),
            _ => true
        };

        if (canLog)
        {
            int maxLenghtForLogs = 10000;
            string responseShorter = response.Substring(0, Math.Min(response.Length, maxLenghtForLogs));
            logger.Log(LogLevel.Info, methodName + " => " + responseShorter);
        }
        else
        {
            logger.Log(LogLevel.Info, methodName);
        }
    }

}