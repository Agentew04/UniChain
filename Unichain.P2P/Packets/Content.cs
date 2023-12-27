using System.Text;

namespace Unichain.P2P.Packets; 

/// <summary>
/// Represents a collection of headers and a payload
/// </summary>
public struct Content {

    /// <summary>
    /// The headers of this content.
    /// </summary>
    public Dictionary<string,string> Headers { get; set; }

    /// <summary>
    /// The binary payload of this content.
    /// </summary>
    public byte[] Payload { get; set; }

    /// <summary>
    /// Writes the current content to the stream
    /// </summary>
    /// <param name="s">The stream to be written onto</param>
    /// <exception cref="NotSupportedException">If the stream is non writable</exception>
    internal readonly void Write(Stream s) {
        if (!s.CanWrite) {
            throw new NotSupportedException("Cannot write to stream");
        }

        using BinaryWriter bw = new(s, Encoding.UTF8, true);

        bw.Write(Headers.Count);
        foreach (var header in Headers) {
            bw.Write(header.Key);
            bw.Write(header.Value);
        }
        bw.Write((uint)Payload.Length);
        bw.Write(Payload);
    }

    /// <summary>
    /// Reads and creates a new content from the stream
    /// </summary>
    /// <param name="s">The stream that has the data</param>
    /// <returns>The newly created content</returns>
    /// <exception cref="NotSupportedException">If the stream is non readable</exception>
    internal static Content Read(Stream s) {
        if (!s.CanRead) {
            throw new NotSupportedException("Cannot read from stream");
        }

        using BinaryReader br = new(s, Encoding.UTF8, true);

        var headers = new Dictionary<string, string>();
        int headerCount = br.ReadInt32();
        for (int i = 0; i < headerCount; i++) {
            string key = br.ReadString();
            string value = br.ReadString();
            headers.Add(key, value);
        }
        uint payloadSize = br.ReadUInt32();
        byte[] payload = br.ReadBytes((int)payloadSize);
        return new Content {
            Headers = headers,
            Payload = payload
        };
    }
}
