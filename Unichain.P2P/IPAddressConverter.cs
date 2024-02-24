using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Unichain.P2P {
    public class IPAddressConverter : JsonConverter<IPAddress> {
        public override IPAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType != JsonTokenType.String) {
                throw new JsonException();
            }

            string ipString = reader.GetString() ?? "";

            bool canParse = IPAddress.TryParse(ipString, out IPAddress? ip);
            if (!canParse) {
                throw new JsonException();
            }
            return ip;
            
        }

        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options) {
            writer.WriteStringValue(value.ToString());
        }
    }
}
