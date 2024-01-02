using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Unichain.P2P.Nodes;
using Unichain.P2P.Packets;
using NLog;
using System.Security.Cryptography;

namespace Unichain.P2P;

/// <summary>
/// A specialized class of <see cref="TcpNode"/> that implements the Unichain protocol
/// </summary>
public class UnichainNode : UdpNode {

    /// <summary>
    /// Logger to log messages to the console
    /// </summary>
    private static readonly Logger logger = LogManager.GetCurrentClassLogger();

    [SuppressMessage("Style", "IDE0290:Usar construtor primário", Justification = "Its ugly")]
    public UnichainNode(int port) : base(port) {
    }

    #region Public Methods

    #endregion

    #region Protected Methods

    protected override Response Process(Request request) {
        RequestMethod method = request.Method;
        Route path = request.Route;
        Response response;
        logger.Info("Received {method} request on {path} from {request.Sender}", method, path, request.Sender);

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

        string json = JsonSerializer.Serialize(sentPeers);
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

        Content ctn = request.Contents[0];

        if (ctn.Headers["contentType"] != "json") {
            return Response.Create()
                .WithProtocolVersion(ProtocolVersion.V1)
                .WithStatusCode(StatusCode.BadRequest)
                .WithContent(Content.Create()
                    .WithHeader("contentType", "text")
                    .WithHeader("encoding", Encoding.UTF8.HeaderName)
                    .WithPayload(Encoding.UTF8.GetBytes("Invalid content type. Must be 'json'"))
                    .Build())
                .Build();
        }

        byte[] payload = ctn.Payload;

        Encoding encoding = Encoding.GetEncoding(ctn.Headers["encoding"]);
        string json = encoding.GetString(payload);
        Address newAddress = JsonSerializer.Deserialize<Address>(json)!;
        logger.Info($"Received new peer {newAddress}.");
        if (!peers.Contains(newAddress) && peers.Count < 100) {
            peers.Add(newAddress);
        }

        return Response.ok;
    }

    #endregion
}
