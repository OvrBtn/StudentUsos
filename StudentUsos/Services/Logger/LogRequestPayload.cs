using System.Text.Json.Serialization;

namespace StudentUsos.Services.Logger;

[JsonSerializable(typeof(LogRequestPayload))]
internal partial class LogRequestPayloadJsonContext : JsonSerializerContext
{ }

internal class LogRequestPayload
{
    public List<LogRecord> Logs { get; set; }
    public string Installation { get; set; }
    public string UserUsosId { get; set; }
}