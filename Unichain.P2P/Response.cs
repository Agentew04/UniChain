using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.P2P;

/// <summary>
/// Represents a response for a request
/// </summary>
public class Response {

    /// <summary>
    /// The status code of the response
    /// </summary>
    public StatusCode StatusCode { get; set; }

    /// <summary>
    /// The Base64 encoded payload of the response
    /// </summary>
    public string Payload { get; set; }

    public Response(StatusCode statusCode, string payload) {
        StatusCode = statusCode;
        Payload = payload;
    }
}
