using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    private readonly Logger logger;

    public UnichainNode(int port) : base(port) {
        logger = new Logger(nameof(UnichainNode) + " " + Port);
    }

    #region Public Methods

    #endregion

    #region Protected Methods

    protected override Response Process(Request request) {
        RequestMethod method = request.Method;
        Route path = request.Route;
        Response response;
        logger.Log($"Received {method} request on {path} from {request.Sender}");

        if (path == Route.Peers && method == RequestMethod.GET) {
            response = GetPeers();
        } else if (path == Route.Peers_Join && method == RequestMethod.POST) {
            response = RegisterNewPeer(request);
        } else {
            response = new Response(StatusCode.NotFound, "");
        }
        return response;
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
        Parallel.ForEach(peers, peer => {
            using TcpClient client = new(peer.Ip, peer.Port);

            SendRequest(request, client);
        });

        return new Response(StatusCode.OK, "");
    }

    #endregion
}
