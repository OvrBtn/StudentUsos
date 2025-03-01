using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudentUsos.Converters
{
    public class JsonStringToIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (int.TryParse(stringValue, out int parsed))
                {
                    return parsed;
                }
                else
                {
                    throw new JsonException($"Unable to convert \"{stringValue}\" to an integer.");
                }
            }

            if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out int intValue))
            {
                return intValue;
            }

            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
