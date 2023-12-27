namespace Unichain.P2P.Packets; 

/// <summary>
/// A class to create new contents for requests and responses.
/// </summary>
public class ContentBuilder {
    private readonly Dictionary<string, string> headers;
    private byte[] payload;

    /// <summary>
    /// Instantiates a new builder for <see cref="Content"/> objects with default information.
    /// </summary>
    public ContentBuilder() {
        headers = new();
        payload = Array.Empty<byte>();
    }

    /// <summary>
    /// Adds a new header to the content.
    /// </summary>
    /// <param name="key">The key of the header</param>
    /// <param name="value">The value of the header</param>
    public ContentBuilder WithHeader(string key, string value) {
        headers.Add(key, value);
        return this;
    }

    /// <summary>
    /// Defines what the payload will be.
    /// </summary>
    /// <param name="payload">The payload</param>
    public ContentBuilder WithPayload(byte[] payload) {
        this.payload = payload;
        return this;
    }

    /// <summary>
    /// Builds the final <see cref="Content"/> object
    /// </summary>
    public Content Build() {
        return new() {
            Headers = headers,
            Payload = payload
        };
    }
}
