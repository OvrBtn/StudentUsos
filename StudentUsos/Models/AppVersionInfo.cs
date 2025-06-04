using System.Text.Json.Serialization;

namespace StudentUsos.Models;

[JsonSerializable(typeof(List<AppVersionInfo>))]
public partial class AppVersionInfoJsonContext : JsonSerializerContext
{ }

public class AppVersionInfo
{
    public string Version { get; set; }
    public bool ForceResetLocalData { get; set; }
    public bool ForceSignOut { get; set; }
}
