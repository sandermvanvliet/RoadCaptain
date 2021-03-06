using System;
using Newtonsoft.Json;

namespace RoadCaptain.Adapters
{
    public class SegmentDirectionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is SegmentDirection direction)
            {
                writer.WriteValue(direction.ToString());
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if(objectType == typeof(SegmentDirection) && reader.Value is string direction)
            {
                if (Enum.TryParse(typeof(SegmentDirection), direction, out var parsedDirection))
                {
                    return parsedDirection;
                }

                throw new JsonReaderException($"Unable to convert the value '{direction}' to enum {objectType.Name}");
            }

            return reader.Value;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SegmentDirection);
        }
    }
}