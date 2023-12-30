using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Unichain.P2P.Nodes;
using Unichain.P2P.Packets;

namespace Unichain.P2P;

/// <summary>
/// A specialized class of <see cref="TcpNode"/> that implements the Unichain protocol
/// </summary>
internal class UnichainNode : TcpNode {

    /// <summary>
    /// Logger to log messages to the console
    /// </summary>
    private readonly Logger logger;

    [SuppressMessage("Style", "IDE0290:Usar construtor primário", Justification = "Its ugly")]
    public UnichainNode(int port) : base(port) {
        logger = new Logger(nameof(UnichainNode) + " " + port);
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
            response = new ResponseBuilder()
                .WithStatusCode(StatusCode.NotFound)
                .Build();
        }
        return response;
    }

    #endregion

    #region Private Methods

    private Response GetPeers() {
        List<Address> sentPeers = new(peers) {
            address
        };

        var json = JsonSerializer.Serialize(sentPeers);
        var bytes = Encoding.UTF8.GetBytes(json);
        ResponseBuilder builder = new();
        Response response = builder
            .WithStatusCode(StatusCode.OK)
            .WithContent(new ContentBuilder()
                .WithHeader("contentType","json")
                .WithHeader("encoding", Encoding.UTF8.HeaderName)
                .WithPayload(bytes)
                .Build())
            .Build();
        return response;
    }

    private Response RegisterNewPeer(Request request) {
        byte[] payload;
        try {
            payload = request.Contents.First(x => x.Headers["contentType"] == "json").Payload;
        }catch (InvalidOperationException) {
            return new ResponseBuilder()
                .WithStatusCode(StatusCode.BadRequest)
                .WithContent(new ContentBuilder()
                    .WithHeader("error", "Missing json payload")
                    .Build())
                .Build();
        }

        Encoding encoding = Encoding.GetEncoding(request.Contents.First(x => x.Headers["encoding"] != null).Headers["encoding"]);
        string json = encoding.GetString(payload);
        Address newAddress = JsonSerializer.Deserialize<Address>(json)!;
        logger.Log($"Received new peer {newAddress}.");
        if (!peers.Contains(newAddress) && peers.Count < 100) {
            peers.Add(newAddress);
        }

        return Response.ok;
    }

    #endregion
}
