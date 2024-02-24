namespace Unichain.P2P.Packets;

public enum CompressionMode : byte
{
    Brotli,
    Deflate,
    GZip,
    ZLib
}
