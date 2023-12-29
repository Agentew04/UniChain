using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unichain.P2P {
    public static class StreamExtensions {
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
    }
}
