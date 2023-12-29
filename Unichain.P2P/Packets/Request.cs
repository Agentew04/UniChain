using System.Security.Cryptography;
using System.Text;

namespace Unichain.P2P.Packets;

/// <summary>
/// Represents a request method from a node
/// </summary>
public readonly struct Request
{
    /// <summary>
    /// The protocol version that the sender is using. 
    /// </summary>
    public byte ProtocolVersion { get; init; }

    /// <summary>
    /// The method of the request
    /// </summary>
    public RequestMethod Method { get; init; }

    /// <summary>
    /// The URI of the request
    /// </summary>
    public Route Route { get; init; }

    /// <summary>
    /// Identification information of the sender
    /// </summary>
    public Address Sender { get; init; }

    /// <summary>
    /// Defines if the current request is a broadcast request. If true,
    /// the node should not send a response and should spread it across
    /// all its known peers.
    /// </summary>
    public bool IsBroadcast { get; init; }

    /// <summary>
    /// A list with all the <see cref="Content"/> present in this request.
    /// </summary>
    public List<Content> Contents { get; init; }

    private static readonly Logger logger = new(nameof(Request));

    /// <summary>
    /// Writes the current request to a stream
    /// </summary>
    /// <param name="s">The stream that will receive the data</param>
    /// <exception cref="NotSupportedException">Thrown when the stream doesnt support writes</exception>
    public readonly void Write(Stream s) {
        if (!s.CanWrite) {
            throw new NotSupportedException("Cannot write to the stream");
        }

        using BinaryWriter writer = new(s, Encoding.UTF8, true);

        writer.Write(ProtocolVersion); // 1 byte
        writer.Write((byte)Method); // 1 byte
        writer.Write(IsBroadcast); // 1 byte
        writer.Write(0b00000000); // 1 byte (reserved)
        writer.Write(Route);
        Sender.Write(s);
        writer.Write((ushort)Contents.Count); // 2 bytes
        foreach (Content content in Contents) {
            content.Write(s);
        }
        writer.Write(GetHash()); // 32 bytes SHA256 hash
    }

    /// <summary>
    /// Reads a request from a stream
    /// </summary>
    /// <param name="s">The stream that has the data</param>
    /// <exception cref="NotSupportedException">Thrown when the stream doesnt support read</exception>
    /// <returns>The request that has been read/returns>
    public static Request Read(Stream s) {
        if (!s.CanRead) {
            throw new NotSupportedException("Cannot read from the stream");
        }

        using BinaryReader reader = new(s, Encoding.UTF8, true);

        byte protocolVersion = reader.ReadByte();
        RequestMethod method = (RequestMethod)reader.ReadByte();
        bool isBroadcast = reader.ReadBoolean();
        reader.ReadByte(); // reserved
        Route route = reader.ReadString();
        Address sender = Address.Read(s);
        ushort contentCount = reader.ReadUInt16();
        List<Content> contents = [];
        for (int i = 0; i < contentCount; i++) {
            contents.Add(Content.Read(s));
        }
        byte[] hash = reader.ReadBytes(SHA256.HashSizeInBytes);

        Request request = new() {
            ProtocolVersion = protocolVersion,
            Method = method,
            IsBroadcast = isBroadcast,
            Route = route,
            Sender = sender,
            Contents = contents
        };

        if (!request.GetHash().SequenceEqual(hash)) {
            logger.LogWarning("Hashes from Request does not match");
        }

        return request;
    }

    /// <summary>
    /// Gets a hash code that represents this object
    /// </summary>
    /// <returns></returns>
    public Span<byte> GetHash() {
        byte[] strBytes = Encoding.UTF8.GetBytes(ToString());
        Span<byte> hash = new byte[SHA256.HashSizeInBytes];
        SHA256.HashData(strBytes, hash);
        return hash;
    }

    /// <summary>
    /// Creates a string representation of this object
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        List<string> hashes = [];
        foreach (var content in Contents) {
            hashes.Add(Convert.ToBase64String(content.GetHash()));
        }
        return $"{ProtocolVersion}-{Method}-{Route}-{Sender}-{IsBroadcast}-{string.Join(",", hashes)}";
    }
}
