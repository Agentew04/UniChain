using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Unichain.P2P {
    public class Address {

        /// <summary>
        /// The unique identifier of the node
        /// </summary>
        [JsonPropertyName("nodeId")]
        public Guid NodeId { get; set; }

        /// <summary>
        /// The IP address used to reach this node.
        /// Preferably the IpV6 address if available.
        /// </summary>
        [JsonPropertyName("ip")]
        public string Ip { get; }

        /// <summary>
        /// The port that this node will be listening on
        /// </summary>
        [JsonPropertyName("port")]
        public int Port { get; }

        /// <summary>
        /// The .NET object representation of the IP address
        /// </summary>
        [JsonIgnore]
        public IPAddress IPAddress => IPAddress.Parse(Ip);

        /// <summary>
        /// A combination of endpoint and IP address
        /// </summary>
        [JsonIgnore]
        public IPEndPoint EndPoint => new(IPAddress, Port);

        public Address(string ip, int port) {
            Ip = ip;
            Port = port;
        }

        [DebuggerStepThrough]
        public override string ToString() => $"{Ip}:{Port}";
    }
}
