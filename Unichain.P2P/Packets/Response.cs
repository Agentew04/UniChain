using System.Text;

namespace Unichain.P2P.Packets;

/// <summary>
/// Represents a response for a request
/// </summary>
public struct Response {

    /// <summary>
    /// The protocol version that this node is using. Used to the sender
    /// troubleshoot issues if he has an different version of the protocol.
    /// </summary>
    public ProtocolVersion ProtocolVersion { get; set; }

    /// <summary>
    /// The status code of the response
    /// </summary>
    public StatusCode StatusCode { get; set; }

    /// <summary>
    /// The Base64 encoded payload of the response
    /// </summary>
    public Content Content { get; set; }

    /// <summary>
    /// Writes the response to a stream.
    /// </summary>
    /// <param name="s">The destination stream</param>
    /// <exception cref="NotSupportedException">If the <paramref name="s"/> is non writable</exception>
    internal readonly void Write(Stream s) {
        if (!s.CanWrite) {
            throw new NotSupportedException("Cannot write to stream");
        }

        using BinaryWriter bw = new(s, Encoding.UTF8, true);

        bw.Write((byte)ProtocolVersion);
        bw.Write((ushort)StatusCode);
        Content.Write(s);
    }

    /// <summary>
    /// Reads and creates a Response object from a stream
    /// </summary>
    /// <param name="s">The stream that has the data</param>
    /// <returns>The object created</returns>
    /// <exception cref="NotSupportedException">If the <paramref name="s"/> is non readable</exception>
    internal static Response Read(Stream s) {
        if (!s.CanRead) {
            throw new NotSupportedException("Cannot read from stream");
        }

        using BinaryReader br = new(s, Encoding.UTF8, true);

        ProtocolVersion protocolVersion = (ProtocolVersion)br.ReadByte();
        StatusCode statusCode = (StatusCode)br.ReadUInt16();
        Content content = Content.Read(s);
        return new Response {
            ProtocolVersion = protocolVersion,
            StatusCode = statusCode,
            Content = content
        };
    }

    #region Preset Responses

    /// <summary>
    /// An <see cref="StatusCode.OK"/> response with no content
    /// </summary>
    public static readonly Response ok = new() {
        ProtocolVersion = ProtocolVersion.V1,
        StatusCode = StatusCode.OK,
        Content = Content.empty
    };

    #endregion
}
