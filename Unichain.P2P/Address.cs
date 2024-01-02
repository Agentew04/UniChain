using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;

namespace Unichain.P2P; 

/// <summary>
/// A class that represents an address of a node and how to reach it
/// </summary>
public class Address {

    /// <summary>
    /// The unique identifier of the node
    /// </summary>
    [JsonPropertyName("nodeId")]
    public Guid NodeId { get; set; }

    /// <summary>
    /// The public ip address of this node. Others nodes normally use
    /// this one
    /// </summary>
    [JsonPropertyName("publicIp")]
    [JsonConverter(typeof(IPAddressConverter))]
    public IPAddress PublicIp { get; set; }

    /// <summary>
    /// The private ip of this node. Used if two nodes are in the
    /// same network. Just check if the Public ip is the same on both
    /// </summary>
    [JsonPropertyName("privateIp")]
    [JsonConverter(typeof(IPAddressConverter))]
    public IPAddress PrivateIp { get; set; }

    /// <summary>
    /// The port that this node will be listening on
    /// </summary>
    [JsonPropertyName("port")]
    public int Port { get; }

    /// <summary>
    /// Writes this address to a stream
    /// </summary>
    /// <param name="s">The stream to write to</param>
    /// <exception cref="NotSupportedException"></exception>
    public void Write(Stream s) {
        if (!s.CanWrite) {
            throw new NotSupportedException("Cannot write to this stream");
        }

        using BinaryWriter writer = new(s, Encoding.UTF8, true);

        writer.Write(NodeId);
        writer.Write(PublicIp.ToString());
        writer.Write(PrivateIp.ToString());
        writer.Write(Port);
    }

    /// <summary>
    /// Reads an address from a stream.
    /// </summary>
    /// <param name="s">The stream that has the data</param>
    /// <exception cref="NotSupportedException"></exception>
    /// <returns>The address read</returns>
    public static Address Read(Stream s) {
        if(!s.CanRead) {
            throw new NotSupportedException("Cannot read from this stream");
        }

        using BinaryReader reader = new(s, Encoding.UTF8, true);

        Guid nodeId = reader.ReadGuid();
        IPAddress publicIp = IPAddress.Parse(reader.ReadString());
        IPAddress privateIp = IPAddress.Parse(reader.ReadString());
        int port = reader.ReadInt32();

        return new Address(nodeId, publicIp, privateIp, port);
    }

    /// <summary>
    /// Creates a new address with the given parameters
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="publicIp"></param>
    /// <param name="privateIp"></param>
    /// <param name="port"></param>
    public Address(Guid nodeId, IPAddress publicIp, IPAddress privateIp, int port) {
        NodeId = nodeId;
        PublicIp = publicIp;
        PrivateIp = privateIp;
        Port = port;
    }
}
