namespace Unichain.P2P.Packets; 

/// <summary>
/// A class to craft new requests.
/// </summary>
public class RequestBuilder {
    private byte protocolVersion;
    private RequestMethod method;
    private Route? route;
    private Address? sender;
    private bool isBroadcast;
    private List<Content> contents;

    /// <summary>
    /// Creates a new builder for <see cref="Request"/> objects with default information.
    /// </summary>
    public RequestBuilder()
    {
        contents = new();
    }

    /// <summary>
    /// Defines the protocol version that the request will use
    /// </summary>
    /// <param name="protocolVersion">The protocol version</param>
    public RequestBuilder WithProtocolVersion(byte protocolVersion) {
        this.protocolVersion = protocolVersion;
        return this;
    }

    /// <summary>
    /// Defines the method that the request will use
    /// </summary>
    /// <param name="method">The method</param>
    public RequestBuilder WithMethod(RequestMethod method) {
        this.method = method;
        return this;
    }

    /// <summary>
    /// Defines the route of the request
    /// </summary>
    /// <param name="route">The route that this request will be forwarded</param>
    public RequestBuilder WithRoute(Route route) {
        this.route = route;
        return this;
    }

    /// <summary>
    /// Defines the sender of the request
    /// </summary>
    /// <param name="sender">The sender</param>
    public RequestBuilder WithSender(Address sender) {
        this.sender = sender;
        return this;
    }

    /// <summary>
    /// Defines a request to be broadcasted across the entire network
    /// </summary>
    public RequestBuilder WithBroadcast() {
        this.isBroadcast = true;
        return this;
    }

    /// <summary>
    /// Sets a specific state for the <see cref="Request"/> to be broadcasted or not
    /// </summary>
    /// <param name="isBroadcast">If the request should be a broadcast or not</param>
    public RequestBuilder WithBroadcast(bool isBroadcast) {
        this.isBroadcast = isBroadcast;
        return this;
    }

    /// <summary>
    /// Adds many <see cref="Content"/> to the request at once.
    /// <b>This method will override any previous contents added to the request.</b>
    /// </summary>
    /// <param name="contents">The contents to be added</param>
    public RequestBuilder WithContents(List<Content> contents) {
        this.contents = contents;
        return this;
    }

    /// <summary>
    /// Adds many <see cref="Content"/> to the request at once 
    /// using an <see cref="Action"/>, preferably a lambda expression
    /// </summary>
    /// <param name="contents">The action that adds all contents</param>
    public RequestBuilder WithContents(Action<List<Content>> contents) {
        contents(this.contents);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="Content"/> to the request
    /// </summary>
    /// <param name="content">The content to be added</param>
    public RequestBuilder WithContent(Content content) {
        this.contents.Add(content);
        return this;
    }

    /// <summary>
    /// Creates a <see cref="Request"/> object with the current builder configuration
    /// </summary>
    /// <exception cref="InvalidOperationException">If the route or sender is not defined</exception>
    public Request Build() {
        if(route is null) {
            throw new InvalidOperationException("The route must be defined");
        }

        if(sender is null) {
            throw new InvalidOperationException("The sender must be defined");
        }

        return new Request {
            ProtocolVersion = protocolVersion,
            Method = method,
            Route = route,
            Sender = sender,
            IsBroadcast = isBroadcast,
            Contents = contents
        };
    }
}
