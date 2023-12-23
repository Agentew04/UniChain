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
    public RequestMethod Method { get; set; }

    /// <summary>
    /// The remote address of the sender
    /// </summary>
    public EndPoint RemoteEndPoint { get; set; }

    /// <summary>
    /// The URI of the request
    /// </summary>
    public Uri Uri { get; set; }

    /// <summary>
    /// The Base64 encoded payload of the request
    /// </summary>
    public string Payload { get; set; }

    /// <summary>
    /// Shortcut to get the payload data if it is a text
    /// </summary>
    public string TextPayload => Encoding.UTF8.GetString(Convert.FromBase64String(Payload));

    public Request(RequestMethod method, Uri uri, string payload, EndPoint remoteEndpoint) { 
        Method = method;
        Uri = uri;
        Payload = payload;
        RemoteEndPoint = remoteEndpoint;
    }
}
