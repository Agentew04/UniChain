using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.P2P;

/// <summary>
/// Represents a request method from a node
/// </summary>
public class Request {

    /// <summary>
    /// The method of the request
    /// </summary>
    public required RequestMethod Method { get; set; }

    /// <summary>
    /// The address of the sender
    /// </summary>
    public Address Sender { get; set; }

    /// <summary>
    /// Defines if this request is a broadcast. It should be propagated across the network and
    /// a response should not matter.
    /// </summary>
    public bool IsBroadcast { get; set; }
    
    /// <summary>
    /// The URI of the request
    /// </summary>
    public required Route Route { get; set; }

    /// <summary>
    /// The Base64 encoded payload of the request
    /// </summary>
    public string Payload { get; set; } = "";

    /// <summary>
    /// Shortcut to get the payload data if it is a text
    /// </summary>
    public string TextPayload => Encoding.UTF8.GetString(Convert.FromBase64String(Payload));

    [Obsolete("Use the parameterless constructor.")]
    public Request(RequestMethod method, Route route, string payload, IPEndPoint sender) { 
        Method = method;
        Route = route;
        Payload = payload;
        Sender = new Address(sender.Address.ToString(), sender.Port);
    }

    public Request() {

    }
}
