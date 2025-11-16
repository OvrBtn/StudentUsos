using System.Globalization;

namespace StudentUsos.Services.ServerConnection;

public class GuestServerConnectionManager : IServerConnectionManager
{
    public static string MethodToFileName(string methodName, Dictionary<string, string> arguments)
    {
        string index = string.Empty;
        if (methodName.Contains("calendar/search"))
        {
            DateTime startDate = new(2025, 11, 1);
            DateTime.TryParseExact(arguments["start_date"], "yyyy-MM-dd", null, DateTimeStyles.None, out DateTime dateTimeArg);
            int startMonth = startDate.Year * 12 + startDate.Month;
            int argMonths = dateTimeArg.Year * 12 + dateTimeArg.Month;
            var months = Math.Abs(startMonth - argMonths) + 1;
            index = "_" + months.ToString();
        }

        string fileName = "DummyServerResponses/" + methodName.Replace('/', '_') + index + ".json";
        return fileName;
    }

    public async Task<string?> SendRequestToUsosAsync(string methodName, Dictionary<string, string> arguments, Action<string>? onRequestFinished = null, int timeout = 10)
    {
        string fileName = MethodToFileName(methodName, arguments);
        var content = await Utilities.ReadRawFileAsync(fileName);
        return content;
    }


#pragma warning disable CS8603
    public Task<RequestResult?> SendAuthorizedGetRequestAsync(string endpoint, Dictionary<string, string> requestPayload, AuthorizationMode authorization, int timeout = 10)
    {
        return Task.FromResult<RequestResult?>(null);
    }

    public Task<RequestResult?> SendAuthorizedPostRequestAsync(string endpoint, string postBody, AuthorizationMode authorization, int timeout = 10)
    {
        return Task.FromResult<RequestResult?>(null);
    }

    public Task<RequestResult?> SendAuthorizedPostRequestAsync(string endpoint, Dictionary<string, string> requestPayload, AuthorizationMode authorization, int timeout = 10)
    {
        return Task.FromResult<RequestResult?>(null);
    }

    public Task<RequestResult?> SendGetRequestAsync(string endpoint, Dictionary<string, string> requestPayload, Dictionary<string, string>? additionalDataToHashButNotSend = null, int timeout = 10)
    {
        return Task.FromResult<RequestResult?>(null);
    }

    public Task<RequestResult?> SendPostRequestAsync(string endpoint, string postBody, Dictionary<string, string> requestHeaders, Dictionary<string, string>? additionalDataToHashButNotSend = null, int timeout = 10)
    {
        return Task.FromResult<RequestResult?>(null);
    }
#pragma warning restore
}
