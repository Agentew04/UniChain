using LiteNetLib;
using LiteNetLib.Utils;
using NLog;
using NLog.Fluent;
using NLog.Targets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;
using Unichain.P2P.Packets;

namespace Unichain.P2P.Nodes;

/// <summary>
/// Class to implement a P2P network node using the a reliable UDP protocol
/// implemented by <see href="https://github.com/RevenantX/LiteNetLib"/>
/// </summary>
public class RudpNode : Node {

    #region Variables

    private const string ProtocolKey = "Unichain";
    private EventBasedNetListener? listener;
    private NetManager? server;
    private NetManager? client;
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    private readonly Dictionary<Address, NetPeer> connectedPeers;
    private readonly List<RudpP2PPacket> receivedPackets;

    #endregion

    /// <summary>
    /// Creates a new instance of a node using RUDP as the communication protocol
    /// </summary>
    /// <param name="port">The port that this node will listen to new requests</param>
    public RudpNode(int port) : base(port) {
        connectedPeers = [];
        receivedPackets = new();
    }

    public override void Start(Address? bootnode) {
        listener = new EventBasedNetListener();
        server = new NetManager(listener);
        client = new NetManager(listener);
        base.Start(bootnode);
    }

    public override void Stop() {
        base.Stop();
        if(server is not null) {
            server.Stop();
            server = null;
        } else {
            logger.Warn("The node is already stopped");
        }
    }

    protected override Response Process(Request request) {
        logger.Info($"Received {request.GetType().Name} from {address.Normalize(request.Sender)}");

        return (string)request.Route switch {
            "/ping" => Response.ok,
            "/peers" => Response.ok,
            "/peers/join" => Response.ok,
            _ => Response.notFound
        };
    }

    #region R/W Requests/Responses

    protected override Request ReadRequest(Address address) {
        var possible = receivedPackets
            .Where(packet => packet.packetType == PacketType.Request)
            .Where(packet => packet.sender == address);

        if(possible.Count() == 0) {
            // todo aaaaaaaaaaaaaaaa
        }

        throw new NotImplementedException();
    }

    protected override Response ReadResponse(Address address) {
        throw new NotImplementedException();
    }

    protected override void SendRequest(Request request, Address address) {
        // we are not connected yet to this peer
        NetPeer? peer = null;
        if (!connectedPeers.ContainsKey(address)) { // do not fix CA1864
            peer = server?.Connect(new IPEndPoint(this.address.Normalize(address), address.Port), ProtocolKey);
            if (peer is not null) {
                connectedPeers.Add(address, peer);
            } else {
                logger.Error("Failed to connect to {address}", address);
                return;
            }
        }

        // send packet
        peer = connectedPeers[address];
        using MemoryStream ms = new();
        request.Write(ms);
        peer.Send(ms.ToArray(), DeliveryMethod.ReliableOrdered);
    }

    protected override void SendResponse(Response response, Address address) {
        // we are not connected yet to this peer
        NetPeer? peer = null;
        if (!connectedPeers.ContainsKey(address)) { // do not fix CA1864
            logger.Debug("Not yet connected to {address}, connecting...", address);
            peer = server?.Connect(new IPEndPoint(this.address.Normalize(address), address.Port), ProtocolKey);
            if (peer is not null) {
                connectedPeers.Add(address, peer);
            } else {
                logger.Error("Failed to connect to {address}", address);
                return;
            }
        }

        // send packet
        peer = connectedPeers[address];
        using MemoryStream ms = new();
        response.Write(ms);
        peer.Send(ms.ToArray(), DeliveryMethod.ReliableOrdered);
    }

    #endregion


    protected async override void ThreadMain() {
        server?.Start(address.Port);
        
        listener!.ConnectionRequestEvent += request => {
            request.AcceptIfKey("Unichain");
            logger.Debug("Received connection request from {endpoint}. Accepting it", request.RemoteEndPoint);
        };

        listener!.PeerConnectedEvent += peer => {
            logger.Debug("Received Peer connection from {endpoint}. Receiving data from him",
                peer.Address);

            NetDataWriter writer = new();                       // Create writer class
            writer.Put("Hello client!");                        // Put some string
            peer.Send(writer, DeliveryMethod.ReliableOrdered);  // Send with reliability
        };

        listener!.NetworkReceiveEvent += PacketReceived;

        while (!cancellationTokenSource.Token.IsCancellationRequested) {
            server?.PollEvents();
            await Task.Delay(15);
        }
    }

    private void PacketReceived(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod) {
        byte[] packetData = reader.RawData;
        PacketType packetType = (PacketType)packetData[1];

        using MemoryStream ms = new(packetData);

        Address sender;
        object packet;
        if(packetType == PacketType.Request) {
            packet = Request.Read(ms);
            sender = ((Request)packet).Sender;
        } else if(packetType == PacketType.Response){
            packet = Response.Read(ms);
            sender = address;
        } else {
            logger.Warn("Received an unknown packet type: {packetType}. Dropping", packetType);
            return;
        }

        receivedPackets.Add(new RudpP2PPacket {
            peer = peer,
            packetType = packetType,
            packet = packet,
            sender = sender
        });
        reader.Recycle();
    }

    private struct RudpP2PPacket {
        public NetPeer peer;
        public Address sender; // the sender of the packet
        public PacketType packetType;
        public object packet; // response or request
    }
}
