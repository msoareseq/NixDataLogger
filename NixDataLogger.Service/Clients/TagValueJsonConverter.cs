using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NixDataLogger.Service.Entities;

namespace NixDataLogger.Service.Clients
{
    internal class TagValueJsonConverter : JsonConverter<object>
    {
        public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }
            else if (reader.TokenType == JsonTokenType.Number)
            {
                var numberValue = reader.GetDouble();
                return numberValue;
            }
            else if (reader.TokenType == JsonTokenType.True || reader.TokenType == JsonTokenType.False)
            {
                return reader.GetBoolean();
            }
            else
            {
                return null;
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            throw new Exception("Writing not supported on tags");
        }
    }
}
