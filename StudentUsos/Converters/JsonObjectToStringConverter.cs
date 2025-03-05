using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos.Converters;

public class JsonObjectToStringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {

        using (var jsonDocument = JsonDocument.ParseValue(ref reader))
        {
            return jsonDocument.RootElement.GetRawText();
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        using (JsonDocument jsonDocument = JsonDocument.Parse(value))
        {
            jsonDocument.WriteTo(writer);
        }
    }
}