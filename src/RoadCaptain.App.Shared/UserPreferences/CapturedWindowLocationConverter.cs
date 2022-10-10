// Copyright (c) 2022 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

using System;
using System.Drawing;
using Newtonsoft.Json;

namespace RoadCaptain.App.Shared.UserPreferences
{
    internal class CapturedWindowLocationConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (objectType != typeof(CapturedWindowLocation))
            {
                return null;
            }

            if (reader.ValueType == typeof(string))
            {
                // This is the old way of storing the location as a System.Drawing.Point
                // This is fine...
                var point = serializer.Deserialize<Point>(reader);
                
                return new CapturedWindowLocation(point.X, point.Y, false, null, null);
            }

            if (reader.TokenType == JsonToken.StartObject)
            {
                return serializer.Deserialize<CapturedWindowLocation>(reader);
            }

            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(CapturedWindowLocation);
        }
    }
}
