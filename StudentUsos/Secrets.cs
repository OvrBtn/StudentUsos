using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos;

[JsonSerializable(typeof(Secrets))]
public partial class SecretsJsonContext : JsonSerializerContext
{ }

public class Secrets
{
    public static Secrets Default { get; private set; }

    const string FileName = "secrets.json";
    static bool isInitialized = false;

    internal static void Initialize()
    {
        if (isInitialized)
        {
            return;
        }
        using var stream = FileSystem.OpenAppPackageFileAsync(FileName).Result;
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        var deserialized = JsonSerializer.Deserialize(json, SecretsJsonContext.Default.Secrets);
        if (deserialized is null)
        {
            throw new Exception("Deserialized secrets are null");
        }
        Default = deserialized;
        isInitialized = true;
    }

    public string InternalConsumerKey { get; init; }
    public string InternalConsumerKeySecret { get; init; }
    public string ServerUrl { get; init; }
    public string UsosConsumerKey { get; init; }

}