using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    /// <summary>
    /// Blocks while condition is true or timeout occurs.
    /// </summary>
    /// <param name="condition">The condition that will perpetuate the block.</param>
    /// <param name="frequency">The frequency at which the condition will be check, in milliseconds.</param>
    /// <param name="timeout">Timeout in milliseconds.</param>
    /// <exception cref="TimeoutException">Thrown when the timout is exceeded</exception>
    [DebuggerHidden]
    public static async Task WaitWhile(Func<bool> condition, int frequency = 25, int timeout = -1) {
        var waitTask = Task.Run(async () => {
            while (condition()) await Task.Delay(frequency);
        });

        if (waitTask != await Task.WhenAny(waitTask, Task.Delay(timeout)))
            throw new TimeoutException();
    }

    /// <summary>
    /// Blocks until condition is true or timeout occurs.
    /// </summary>
    /// <param name="condition">The break condition.</param>
    /// <param name="frequency">The frequency at which the condition will be checked.</param>
    /// <param name="timeout">The timeout in milliseconds.</param>
    [DebuggerHidden]
    public static async Task WaitUntil(Func<bool> condition, int frequency = 25, int timeout = -1) {
        var waitTask = Task.Run(async () => {
            while (!condition()) await Task.Delay(frequency);
        });

        if (waitTask != await Task.WhenAny(waitTask,
                Task.Delay(timeout)))
            throw new TimeoutException();
    }
}
