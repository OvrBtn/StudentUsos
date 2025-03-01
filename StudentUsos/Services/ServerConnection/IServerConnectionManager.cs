namespace StudentUsos.Services.ServerConnection
{
    public interface IServerConnectionManager
    {
        const int HttpClientDefaultTimeoutSeconds = 10;

        /// <summary>
        /// Sends request to USOS API
        /// </summary>
        /// <param name="methodName">Part of URL pointing to required method e.g. "services/users/user"</param>
        /// <param name="arguments">Method arguments e.g. { { "user_id", "1" } }</param>
        /// <param name="onRequestFinished">Event invoked when response from API was received, with the response as argument</param>
        /// <param name="timeout"></param>
        /// <returns>USOS API response</returns>
        public Task<string?> SendRequestToUsosAsync(string methodName,
            Dictionary<string, string> arguments,
            Action<string> onRequestFinished = null,
            int timeout = HttpClientDefaultTimeoutSeconds);

        /// <summary>
        /// Send GET request to internal server.
        /// Authorization happens without any access tokens.
        /// </summary>
        /// <param name="endpoint">Last segments of url, without /api/v1 e.g. logs/log</param>
        /// <param name="requestPayload">Data to include in url query (after ?)</param>
        /// <param name="additionalDataToHashButNotSend">Only for hashing</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Task<RequestResult?> SendGetRequestAsync(string endpoint,
            Dictionary<string, string> requestPayload,
            Dictionary<string, string> additionalDataToHashButNotSend = null,
            int timeout = HttpClientDefaultTimeoutSeconds);

        /// <summary>
        ///  Send GET request to internal server.
        ///  Works similarly to <see cref="SendGetRequestAsync(string, Dictionary{string, string}, Dictionary{string, string}, int)"/> 
        ///  but allows choosing level of authorization and can include access tokens.
        /// </summary>
        /// <param name="endpoint">Last segments of url, without /api/v1 e.g. logs/log</param>
        /// <param name="requestPayload">Data to include in url query (after ?)</param>
        /// <param name="authorization">Decides which keys and tokens to include in request headers</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Task<RequestResult?> SendAuthorizedGetRequestAsync(string endpoint,
            Dictionary<string, string> requestPayload,
            AuthorizationMode authorization,
            int timeout = HttpClientDefaultTimeoutSeconds);

        /// <summary>
        ///  Send POST request to internal server.
        ///  Authorization happens without any access tokens.
        /// </summary>
        /// <param name="endpoint">Last segments of url, without /api/v1 e.g. logs/log</param>
        /// <param name="postBody"></param>
        /// <param name="requestHeaders">Other than standard authorization headers included automatically allows injecting additional ones</param>
        /// <param name="additionalDataToHashButNotSend">Only for hashing</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Task<RequestResult?> SendPostRequestAsync(string endpoint,
            string postBody,
            Dictionary<string, string> requestHeaders,
            Dictionary<string, string> additionalDataToHashButNotSend = null,
            int timeout = HttpClientDefaultTimeoutSeconds);

        /// <summary>
        ///  Send POST request to internal server.
        ///  Works similarly to <see cref="SendPostRequestAsync(string, string, Dictionary{string, string}, Dictionary{string, string}, int)"/> 
        ///  but allows choosing level of authorization and can include access tokens.
        /// </summary>
        /// <param name="endpoint">Last segments of url, without /api/v1 e.g. logs/log</param>
        /// <param name="postBody"></param>
        /// <param name="authorization">Decides which keys and tokens to include in request headers</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Task<RequestResult?> SendAuthorizedPostRequestAsync(string endpoint,
            string postBody,
            AuthorizationMode authorization,
            int timeout = HttpClientDefaultTimeoutSeconds);

        /// <summary>
        /// Works same as <see cref="SendAuthorizedPostRequestAsync(string, Dictionary{string, string}, AuthorizationMode, int)"/>
        /// but handles serializing post body
        /// </summary>
        /// <param name="endpoint">Last segments of url, without /api/v1 e.g. logs/log</param>
        /// <param name="requestPayload"></param>
        /// <param name="authorization">Decides which keys and tokens to include in request headers</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Task<RequestResult?> SendAuthorizedPostRequestAsync(string endpoint,
            Dictionary<string, string> requestPayload,
            AuthorizationMode authorization,
            int timeout = HttpClientDefaultTimeoutSeconds);
    }
}
