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
    private Logger logger = new(nameof(UnichainNode));

    /// <summary>
    /// A list to record recently sent broadcast messages
    /// </summary>
    private readonly FixedList<string> lastPropagations = new(10);

    public UnichainNode(int port) : base(port) {
    }

    #region Public Methods

    public void Broadcast(string payload) {
        string hash = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(payload)));
        if (lastPropagations.Contains(hash)) {
            logger.Log($"I have already propagated {hash}!");
            return;
        }
        lastPropagations.Add(hash);

        Parallel.ForEach(peers, peer => {
            TcpClient tcpClient = new(peer.IP, peer.Port);
            logger.Log($"Propagating to peer {peer}...");
            SendRequest(new Request(RequestMethod.POST, new Uri("/broadcast"), payload), tcpClient);
            Response resp = ReadResponse(tcpClient);
            if (resp.StatusCode != 200) {
                logger.LogWarning($"Failed to propagate to peer {peer}! Response: {resp.StatusCode}");
            }
            tcpClient.Close();
        });
    }

    #endregion

    #region Protected Methods

    protected override Response Process(Request request) {
        RequestMethod method = request.Method;
        Uri uri = request.Uri;

        logger.Log($"Received {method} request on {path} from {request.RemoteEndPoint}");

        if (path == "/api/peers" && method == "GET") {
            List<Address> peersSent = new();
            peersSent.AddRange(peers);
            peersSent.Add(new Address("localhost", port));

            var json = JsonSerializer.Serialize(peersSent);
            var bytes = Encoding.UTF8.GetBytes(json);
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;
            response.ContentLength64 = bytes.Length;
            await response.OutputStream.WriteAsync(bytes);
            response.Close();
        } else if (path == "/api/peers/join" && method == "POST") {
            StreamReader reader = new(request.InputStream);
            var json = await reader.ReadToEndAsync();
            Address newAddress = JsonSerializer.Deserialize<Address>(json);
            logger.Log($"Received new peer {newAddress}.");
            if (!peers.Contains(newAddress)) {
                peers.Add(newAddress);
            }
            response.StatusCode = 200;
            response.Close();

            // here we ping the new peer
            tcpClient.BaseAddress = new Uri($"http://{newAddress}");
            logger.Log($"Pinging new peer {newAddress}...");
            var pingResponse = await tcpClient.GetAsync("api/ping");
            if (pingResponse.IsSuccessStatusCode) {
                logger.Log($"Successfully pinged new peer {newAddress}");
            } else {
                logger.LogWarning($"Failed to ping new peer {newAddress}! Response: {pingResponse.StatusCode}");
            }
        } else if (path == "/api/propagate" && method == "POST") {
            using StreamReader reader = new(request.InputStream);
            var json = await reader.ReadToEndAsync();
            using JsonDocument document = JsonDocument.Parse(json);
            string message = document.RootElement.GetProperty("message").GetString() ?? "";
            string hash = document.RootElement.GetProperty("hash").GetString() ?? "";
            logger.Log($"Received message {message} with hash {hash}. Broadcasting...");
            await Broadcast(message, hash);
            response.StatusCode = 200;
            response.Close();
        } else if (path == "api/ping" && method == "GET") {
            response.StatusCode = 500;
            response.Close();
        } else if (path == "/api/ping" && method == "GET") {
            // respond with pong
            logger.Log($"Being pinged by {request.RemoteEndPoint}!");
            response.StatusCode = 200;
            logger.Log($"Answered ping by {request.RemoteEndPoint}!");
            response.Close();

            // pong the sender
            tcpClient.BaseAddress = new Uri($"http://{request.RemoteEndPoint}");
            logger.Log($"Ponging {request.RemoteEndPoint}...");
            var pongResponse = await tcpClient.GetAsync("api/pong");
            if (pongResponse.IsSuccessStatusCode) {
                logger.Log($"Successfully ponged {request.RemoteEndPoint}");
            } else {
                logger.LogWarning($"Failed to pong {request.RemoteEndPoint}! Response: {pongResponse.StatusCode}");
            }
        } else if (path == "/api/pong" && method == "GET") {
            logger.Log($"Being ponged by {request.RemoteEndPoint}!");
            response.StatusCode = 200;
            response.Close();
        } else {
            response.StatusCode = 404;
            response.Close();
        }
    }

    #endregion

    #region Private Methods

    #endregion

    public async Task Broadcast(string message, string hash) {
        

        foreach (var peer in peers) {
            Uri baseAddress = new Uri($"http://{peer}");
            tcpClient.BaseAddress = baseAddress;
            var json = JsonSerializer.Serialize(new { message, hash });
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            logger.Log($"Sending message {message} with hash {hash} to {peer}...");
            var response = await tcpClient.PostAsync("api/propagate", content);
            if (!response.IsSuccessStatusCode) {
                logger.LogWarning($"Failed to send message to {peer}! Response: {response.StatusCode}");
            }
        }
    }
}
