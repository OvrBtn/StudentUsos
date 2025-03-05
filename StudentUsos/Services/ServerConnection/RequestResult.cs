namespace StudentUsos.Services.ServerConnection;

public class RequestResult
{
    public RequestResult(bool isSuccess, HttpResponseMessage httpResponseMessage, string response)
    {
        IsSuccess = isSuccess;
        HttpResponseMessage = httpResponseMessage;
        Response = response;
    }

    public bool IsSuccess { get; set; }
    public HttpResponseMessage HttpResponseMessage { get; set; }
    public string Response { get; set; }

}