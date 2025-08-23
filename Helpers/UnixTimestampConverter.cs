using System.Text.Json;
using System.Text.Json.Serialization;

namespace server.Helpers
{
    public class UnixTimestampConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                long unixTime = reader.GetInt64();
                return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
            }
            throw new JsonException("Expected a number for Unix timestamp.");
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            long unixTime = ((DateTimeOffset)value).ToUnixTimeSeconds();
            writer.WriteNumberValue(unixTime);
        }
    }
}
