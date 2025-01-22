// Copyright (c) 2025 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using Newtonsoft.Json;

namespace RoadCaptain.Adapters
{
    public class SegmentDirectionConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is SegmentDirection direction)
            {
                writer.WriteValue(direction.ToString());
            }
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if(objectType == typeof(SegmentDirection) && reader.Value is string direction)
            {
                if (Enum.TryParse<SegmentDirection>(direction, out var parsedDirection))
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
