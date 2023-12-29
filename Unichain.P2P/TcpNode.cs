using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Unichain.P2P.Packets;

namespace Unichain.P2P;
public abstract class TcpNode {

    #region Variables

    /// <summary>
    /// The address that identifies this node.
    /// </summary>
    protected readonly Address address;

    /// <inheritdoc cref="address"/>
    public Address Address => address;

    /// <summary>
    /// A list with all the peers that this node is connected/knows about
    /// </summary>
    protected List<Address> peers = [];

    /// <inheritdoc cref="peers"/>
    public List<Address> Peers => new(peers);

    /// <summary>
    /// Listener to receive messages from other nodes
    /// </summary>
    private readonly TcpListener tcpListener;

    /// <summary>
    /// Logger to log messages to the console
    /// </summary>
    private readonly Logger logger;

    /// <summary>
    /// The internal thread that will run the node.
    /// </summary>
    private readonly Thread thread;

    /// <summary>
    /// Source for the cancellation token
    /// </summary>
    private readonly CancellationTokenSource cancellationTokenSource = new();

    /// <summary>
    /// A list to record recently sent broadcast messages
    /// </summary>
    private readonly FixedList<string> lastPropagations = new(10);

    #endregion

    /// <summary>
    /// Initializes variables for the <see cref="TcpNode"/>
    /// </summary>
    /// <param name="port">The port that this node will listen</param>
    protected TcpNode(int port) {
        address = IpManager.GetCurrentAddress(Guid.NewGuid(), port);

        tcpListener = new(new IPEndPoint(IPAddress.Any, port));
        logger = new Logger(nameof(TcpNode) + " " + port.ToString());
        thread = new(ThreadMain);
    }


    #region Public Methods

    /// <summary>
    /// Starts the internal thread of this node.
    /// </summary>
    /// <param name="bootnode">The bootnode to get peers. If this is null, it will be
    /// a new bootnode. Effectively creating a new network</param>
    /// <returns></returns>
    public void Start(Address? bootnode) {
        if (bootnode is not null) {
            FetchPeers(bootnode);
        }
        logger.Log($"Starting node with {peers.Count} peers...");
        thread.Start();
    }

    /// <summary>
    /// Asks the node to stop acception connections and sending messages
    /// </summary>
    /// <returns></returns>
    public async Task Stop() {
        cancellationTokenSource.Cancel();
        try { 
            await Task.Run(thread.Join);
        }catch(ThreadStateException e) {
            logger.LogError($"Failed to stop node! {e.Message}");
        }catch(ThreadInterruptedException e) {
            logger.LogError($"Failed to stop node! {e.Message}");
        }
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Reads a request from a <see cref="TcpClient"/>
    /// </summary>
    /// <param name="client">The client that sent the request</param>
    /// <returns>The request object</returns>
    protected static Request ReadRequest(TcpClient client) => Request.Read(client.GetStream());

    /// <summary>
    /// Sends a request to a <see cref="TcpClient"/>
    /// </summary>
    /// <param name="request"></param>
    /// <param name="client"></param>
    protected static void SendRequest(Request request, TcpClient client) => request.Write(client.GetStream());

    /// <summary>
    /// Performs the logic for a request. This is run in the internal thread of the node.
    /// </summary>
    /// <param name="request">The Request that was sent</param>
    /// <returns>The response object</returns>
    protected abstract Response Process(Request request);

    /// <summary>
    /// Sends a response to a <see cref="TcpClient"/>
    /// </summary>
    /// <param name="response">The response that will be sent</param>
    /// <param name="client">The client that made the request and will receive the response</param>
    protected static void SendResponse(Response response, TcpClient client) => response.Write(client.GetStream());

    /// <summary>
    /// Reads a response sent from a <see cref="TcpClient"/>
    /// </summary>
    /// <param name="client">The client that received the Request and sent the Response</param>
    /// <returns></returns>
    protected static Response ReadResponse(TcpClient client) => Response.Read(client.GetStream());

    #endregion

    #region Private Methods

    private void ThreadMain() {
        tcpListener.Start();
        logger.Log($"Listening...");

        // the listen loop
        while (!cancellationTokenSource.IsCancellationRequested) {
            TcpClient incoming = tcpListener.AcceptTcpClient();
            NetworkStream inStream = incoming.GetStream();

            // Read the request
            Request request = Request.Read(inStream);

            // Process the request
            Response response = Process(request);

            // Send the response or broadcast
            if (!request.IsBroadcast) {
                response.Write(inStream);
            } else {
                Broadcast(request);
            }

            // Close the connection
            logger.Log($"Closed connection with {((IPEndPoint)incoming.Client.RemoteEndPoint!).Address}");
            incoming.Close();
        }
    }

    /// <summary>
    /// Gets the peers from the bootnode
    /// </summary>
    /// <param name="bootnode">The address of the bootnode</param>
    /// <returns></returns>
    private void FetchPeers(Address bootnode) {
        // get the list of knowns peers from the bootnode
        IPAddress ipAddr = GetNormalizedIp(bootnode);
        
        using (TcpClient tcpClient = new(new IPEndPoint(ipAddr, bootnode.Port))) {
            Request req = new RequestBuilder()
                .WithMethod(RequestMethod.GET)
                .WithRoute(Route.Peers)
                .WithSender(address)
                .Build();
            if (!tcpClient.Connected) {
                tcpClient.Connect(ipAddr, bootnode.Port);
            }

            SendRequest(req, tcpClient);
            Response resp = ReadResponse(tcpClient);

            if (resp.StatusCode != StatusCode.OK) {
                logger.LogError($"Failed to connect to the bootnode! Response: ${resp.StatusCode}");
                return;
            }

            Encoding encoding = Encoding.GetEncoding(resp.Content.Headers["encoding"]);
            string json = encoding.GetString(resp.Content.Payload);
            var addresses = JsonSerializer.Deserialize<List<Address>>(json);
            if (addresses is null) {
                logger.LogError($"Failed to deserialize peers!");
                return;
            }
            logger.Log($"Got {addresses.Count} peers from bootnode");
            peers = addresses;
        }

        // now we send our address to the bootnode
        using (TcpClient tcpClient = new(new IPEndPoint(ipAddr, bootnode.Port))) {
            if (!tcpClient.Connected) {
                tcpClient.Connect(ipAddr, bootnode.Port);
            }
            byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(address));
            Content ctn = new ContentBuilder()
                .WithHeader("encoding", Encoding.UTF8.WebName)
                .WithPayload(payload)
                .Build();

            logger.Log($"Sending our address to the bootnode...");
            Request req = new RequestBuilder()
                .WithMethod(RequestMethod.POST)
                .WithRoute(Route.Peers_Join)
                .WithSender(address)
                .WithContent(ctn)
                .WithBroadcast()
                .Build();

            SendRequest(req, tcpClient);
            lastPropagations.Add(Convert.ToHexString(req.GetHash()));
        }
    }

    /// <summary>
    /// Spreads a broadcast across the network
    /// </summary>
    /// <param name="req">The request that was sent to this machine</param>
    private void Broadcast(Request req) {
        string hash = Convert.ToHexString(req.GetHash());
        if (lastPropagations.Contains(hash)) {
            logger.Log($"I have already propagated {hash}!");
            return;
        }
        lastPropagations.Add(hash);

        Parallel.ForEach(peers, peer => {
            IPAddress ipAddr = GetNormalizedIp(peer);
            using TcpClient tcpClient = new(new IPEndPoint(ipAddr, peer.Port));
            logger.Log($"Broadcasting to peer {peer}...");
            SendRequest(req, tcpClient);
        });
    }

    private IPAddress GetNormalizedIp(Address nodeAddress) {
        if (address.PublicIp.Equals(nodeAddress.PublicIp)) {
            // same network
            if (address.PrivateIp.Equals(nodeAddress.PrivateIp)) {
                // same computer
                return IPAddress.Loopback;
            } else {
                return nodeAddress.PrivateIp;
            }
        } else {
            // different networks
            return nodeAddress.PublicIp;
        }
    }
    #endregion

}
