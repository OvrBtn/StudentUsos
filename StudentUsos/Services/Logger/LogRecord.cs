using System.Globalization;
using System.Text.Json.Serialization;

namespace StudentUsos.Services.Logger
{
    [JsonSerializable(typeof(List<LogRecord>))]
    public partial class LogRecordJsonContext : JsonSerializerContext
    { }

    public class LogRecord
    {
        public string LogLevel { get; set; }
        public string Message { get; set; }
        public string ExceptionMessage { get; set; }
        public string ExceptionSerialized { get; set; }
        public string CallerName { get; set; }
        public string CallerLineNumber { get; set; }
        public string CreationDate { get; set; } = DateTimeOffset.UtcNow.DateTime.ToString(CultureInfo.InvariantCulture);
        public long CreationDateUnix { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
