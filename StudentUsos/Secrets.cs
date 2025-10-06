using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos;

[JsonSerializable(typeof(Secrets))]
public partial class SecretsJsonContext : JsonSerializerContext
{ }

/// <summary>
/// These secrets are not really meant to be secret since they are bundled inside the .apk.
/// Main purpose of this class is to centralize all constants which would be otherwise hardcoded.
/// Any keys to external services should not be stored here.
/// </summary>
//TODO: maybe rename this to Constants
public class Secrets
{
    public static Secrets Default { get; private set; }

    const string FileName = "secrets.json";
    static bool isInitialized = false;

    static Secrets()
    {
        Initialize();
    }

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

    public string InternalConsumerKey { get; set; }
    public string InternalConsumerKeySecret { get; set; }
    public string ServerUrl { get; set; }

}