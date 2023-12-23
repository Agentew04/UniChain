using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Unichain.P2P;

/// <summary>
/// A specialized class of <see cref="TcpNode"/> that implements the Unichain protocol
/// </summary>
internal class UnichainNode : TcpNode {

    /// <summary>
    /// Logger to log messages to the console
    /// </summary>
    private readonly Logger logger = new(nameof(UnichainNode));

    /// <summary>
    /// A list to record recently sent broadcast messages
    /// </summary>
    private readonly FixedList<string> lastPropagations = new(10);

    public UnichainNode(int port) : base(port) {
    }

    #region Public Methods

    public void Broadcast(string payload) {
        string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
        if (lastPropagations.Contains(hash)) {
            logger.Log($"I have already propagated {hash}!");
            return;
        }
        lastPropagations.Add(hash);

        Parallel.ForEach(peers, peer => {
            TcpClient tcpClient = new(peer.IP, peer.Port);
            logger.Log($"Broadcasting to peer {peer}...");
            SendRequest(new Request(RequestMethod.POST, new Uri("/broadcast"), payload, tcpClient.Client.RemoteEndPoint!), tcpClient);
            Response resp = ReadResponse(tcpClient);
            if (resp.StatusCode != StatusCode.OK) {
                logger.LogWarning($"Failed to propagate to peer {peer}! Response: {resp.StatusCode}");
            }
            tcpClient.Close();
        });
    }

    #endregion

    #region Protected Methods

    protected override Task<Response> Process(Request request) {
        RequestMethod method = request.Method;
        Uri uri = request.Uri;
        string path = uri.AbsolutePath;
        Response response;
        logger.Log($"Received {method} request on {uri} from {request.RemoteEndPoint}");

        if (path == "/peers" && method == RequestMethod.GET) {
            response = GetPeers();
        } else if (path == "/peers/join" && method == RequestMethod.POST) {
            response = RegisterNewPeer(request);
        } else if (path == "/broadcast" && method == RequestMethod.POST) {
            response = BroadcastMessage(request);
        } else {
            response = new Response(StatusCode.NotFound, "");
        }
        return Task.FromResult(response);
    }

    #endregion

    #region Private Methods

    private Response GetPeers() {
        List<Address> peersSent = new();
        peersSent.AddRange(peers);
        peersSent.Add(new Address("localhost", port));
        var json = JsonSerializer.Serialize(peersSent);
        var bytes = Encoding.UTF8.GetBytes(json);
        return new Response(StatusCode.OK, Convert.ToBase64String(bytes));
    }

    private Response RegisterNewPeer(Request request) {
        string json = request.TextPayload;
        Address newAddress = JsonSerializer.Deserialize<Address>(json)!;
        logger.Log($"Received new peer {newAddress}.");
        if (!peers.Contains(newAddress)) {
            peers.Add(newAddress);
        }
        // TODO: send the new peer the list of known peers
        return new Response(StatusCode.OK, "");
    }

    private Response BroadcastMessage(Request request) {
        var message = request.TextPayload;
        logger.Log($"Received message {message}. Broadcasting...");
        Broadcast(message);
        return new Response(StatusCode.OK, "");
    }

    #endregion
}
