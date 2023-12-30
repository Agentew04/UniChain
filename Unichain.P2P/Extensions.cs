using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.P2P; 
public static class Extensions {
    public static Guid ReadGuid(this BinaryReader reader) {
        Span<byte> bytes = stackalloc byte[16];
        int read = reader.Read(bytes);
        if (read != 16) {
            throw new EndOfStreamException("Could not read 16 bytes for Guid");
        }
        return new Guid(bytes);
    }
    public static void Write(this BinaryWriter writer, Guid guid) {
        Span<byte> bytes = stackalloc byte[16];
        if (!guid.TryWriteBytes(bytes)) {
            throw new EndOfStreamException("Could not write 16 bytes for Guid");
        }
        writer.Write(bytes);
    }

    /// <summary>
    /// Gets the normalized IP address of a node Address in comparison with a origin node.
    /// </summary>
    /// <param name="origin">The address to compare with</param>
    /// <param name="other">The address to normalize</param>
    /// <returns>The normalized address</returns>
    public static IPAddress Normalize(this Address origin, Address other) {
        if (origin.PublicIp.Equals(other.PublicIp)) {
            // same network
            if (origin.PrivateIp.Equals(other.PrivateIp)) {
                // same computer
                return IPAddress.Loopback;
            } else {
                return other.PrivateIp;
            }
        } else {
            // different networks
            return other.PublicIp;
        }
    }
}
