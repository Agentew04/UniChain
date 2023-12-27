namespace Unichain.P2P.Packets;

/// <summary>
/// A class to craft new responses.
/// </summary>
public class ResponseBuilder {
    private int protocolVersion;
    private StatusCode statusCode;
    private Content content;

    /// <summary>
    /// Defines the protocol version that the response will use
    /// </summary>
    /// <param name="protocolVersion">The protocol version used</param>
    public ResponseBuilder WithProtocolVersion(int protocolVersion) {
        this.protocolVersion = protocolVersion;
        return this;
    }

    /// <summary>
    /// Sets the status code of the response
    /// </summary>
    /// <param name="statusCode">The status code</param>
    public ResponseBuilder WithStatusCode(StatusCode statusCode) {
        this.statusCode = statusCode;
        return this;
    }

    /// <summary>
    /// Defines the content of the response
    /// </summary>
    /// <param name="content">The content of the response</param>
    public ResponseBuilder WithContent(Content content) {
        this.content = content;
        return this;
    }

    /// <summary>
    /// Creates a new <see cref="Response"/> object with the configurations
    /// provided
    /// </summary>
    /// <returns>The new response object</returns>
    public Response Build() {
        return new Response {
            ProtocolVersion = protocolVersion,
            StatusCode = statusCode,
            Content = content
        };
    }
}
