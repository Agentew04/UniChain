using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Unichain.P2P {
    public class Address {

        [JsonPropertyName("ip")]
        public string IP { get; }

        [JsonPropertyName("port")]
        public int Port { get; }

        public Address(string ip, int port) {
            IP = ip;
            Port = port;
        }

        [DebuggerStepThrough]
        public override string ToString() => $"{IP}:{Port}";
    }
}
