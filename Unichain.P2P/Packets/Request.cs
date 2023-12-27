namespace Unichain.P2P.Packets;

/// <summary>
/// Represents a request method from a node
/// </summary>
public struct Request
{
    /// <summary>
    /// The protocol version that the sender is using. 
    /// </summary>
    public int ProtocolVersion { get; set; }

    /// <summary>
    /// The method of the request
    /// </summary>
    public RequestMethod Method { get; set; }

    /// <summary>
    /// The URI of the request
    /// </summary>
    public Route Route { get; set; }

    /// <summary>
    /// Identification information of the sender
    /// </summary>
    public Address Sender { get; set; }

    /// <summary>
    /// Defines if the current request is a broadcast request. If true,
    /// the node should not send a response and should spread it across
    /// all its known peers.
    /// </summary>
    public bool IsBroadcast { get; set; }

    /// <summary>
    /// A list with all the <see cref="Content"/> present in this request.
    /// </summary>
    public List<Content> Contents { get; set; }

    // TODO: implement read/write in request
    // must implement Address read/write first and public/private IP stuff
}
