namespace StudentUsos.Services.ServerConnection;

public class SwitchableServerConnectionManager : IServerConnectionManager
{
    static SwitchableServerConnectionManager instance;
    public SwitchableServerConnectionManager(IServerConnectionManager defaultManager)
    {
        if (instance != null)
        {
            throw new ApplicationException("SwitchableServerConnectionManager was already initalized");
        }
        instance = this;
        serverConnectionManager = defaultManager;
    }

    IServerConnectionManager serverConnectionManager;
    public void SwitchImplementation(IServerConnectionManager serverConnectionManager)
    {
        this.serverConnectionManager = serverConnectionManager;
    }

    public Task<RequestResult?> SendAuthorizedGetRequestAsync(string endpoint, Dictionary<string, string> requestPayload, AuthorizationMode authorization, int timeout = 10)
    {
        return serverConnectionManager.SendAuthorizedGetRequestAsync(endpoint, requestPayload, authorization, timeout);
    }

    public Task<RequestResult?> SendAuthorizedPostRequestAsync(string endpoint, string postBody, AuthorizationMode authorization, int timeout = 10)
    {
        return serverConnectionManager.SendAuthorizedPostRequestAsync(endpoint, postBody, authorization, timeout);
    }

    public Task<RequestResult?> SendAuthorizedPostRequestAsync(string endpoint, Dictionary<string, string> requestPayload, AuthorizationMode authorization, int timeout = 10)
    {
        return serverConnectionManager.SendAuthorizedPostRequestAsync(endpoint, requestPayload, authorization, timeout);
    }

    public Task<RequestResult?> SendGetRequestAsync(string endpoint, Dictionary<string, string> requestPayload, Dictionary<string, string>? additionalDataToHashButNotSend = null, int timeout = 10)
    {
        return serverConnectionManager.SendGetRequestAsync(endpoint, requestPayload, additionalDataToHashButNotSend, timeout);
    }

    public Task<RequestResult?> SendPostRequestAsync(string endpoint, string postBody, Dictionary<string, string> requestHeaders, Dictionary<string, string>? additionalDataToHashButNotSend = null, int timeout = 10)
    {
        return serverConnectionManager.SendPostRequestAsync(endpoint, postBody, requestHeaders, additionalDataToHashButNotSend, timeout);
    }

    public async Task<string?> SendRequestToUsosAsync(string methodName, Dictionary<string, string> arguments, Action<string>? onRequestFinished = null, int timeout = 10)
    {
        var result = await serverConnectionManager.SendRequestToUsosAsync(methodName, arguments, onRequestFinished, timeout);
        return result;
    }
}
