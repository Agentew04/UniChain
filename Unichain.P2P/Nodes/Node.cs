using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using Unichain.P2P.Packets;

namespace Unichain.P2P.Nodes;

/// <summary>
/// A generic node to implement P2P communication for the Unichain network.
/// </summary>
public abstract class Node
{

    #region Variables

    /// <inheritdoc cref="address"/>
    public Address Address => address;

    /// <inheritdoc cref="peers"/>
    public List<Address> Peers => new(peers);


    /// <summary>
    /// The address that identifies this node.
    /// </summary>
    protected readonly Address address;

    /// <summary>
    /// A list with all the peers that this node is connected/knows about
    /// </summary>
    protected List<Address> peers = [];

    /// <summary>
    /// The internal thread that will run the node.
    /// </summary>
    protected readonly Thread thread;

    /// <summary>
    /// Source for the cancellation token
    /// </summary>
    protected readonly CancellationTokenSource cancellationTokenSource = new();


    /// <summary>
    /// A list to record recently sent broadcast messages
    /// </summary>
    private readonly FixedList<string> lastPropagations = new(10);

    /// <summary>
    /// Class to log messages to the console
    /// </summary>
    private Logger logger;

    #endregion

    #region Concrete Methods

    protected Node(int port)
    {
        address = IpManager.GetCurrentAddress(Guid.NewGuid(), port);
        thread = new(ThreadMain);
        logger = new(nameof(Node) + " " + port.ToString());
    }

    /// <summary>
    /// Starts the internal thread of this node.
    /// </summary>
    /// <param name="bootnode">The bootnode to get peers. If this is null, it will be
    /// a new bootnode. Effectively creating a new network</param>
    /// <returns></returns>
    public virtual void Start(Address? bootnode)
    {
        if (bootnode is not null)
        {
            FetchPeers(bootnode);
        }
        logger.Log($"Starting node with {peers.Count} peers...");
        thread.Start();
    }

    /// <summary>
    /// Asks the node to stop acception connections and sending messages
    /// </summary>
    public void Stop()
    {
        cancellationTokenSource.Cancel();
        try
        {
            thread.Join();
        }
        catch (ThreadStateException e)
        {
            logger.LogError($"Failed to stop node! {e.Message}");
        }
        catch (ThreadInterruptedException e)
        {
            logger.LogError($"Failed to stop node! {e.Message}");
        }
    }

    /// <summary>
    /// Gets a list of peers from the bootnode and broadcasts our address to them.
    /// </summary>
    /// <param name="bootnode">The address of the bootnode</param>
    private void FetchPeers(Address bootnode)
    {
        // get the list of knowns peers from the bootnode
        Request req = new RequestBuilder()
            .WithMethod(RequestMethod.GET)
            .WithRoute(Route.Peers)
            .WithSender(address)
            .Build();
        SendRequest(req, bootnode);
        Response resp = ReadResponse(bootnode);

        if (resp.StatusCode != StatusCode.OK)
        {
            logger.LogError($"Failed to connect to the bootnode! Response: ${resp.StatusCode}");
            return;
        }

        Encoding encoding = Encoding.GetEncoding(resp.Content.Headers["encoding"]);
        string json = encoding.GetString(resp.Content.Payload);
        var addresses = JsonSerializer.Deserialize<List<Address>>(json);
        if (addresses is null)
        {
            logger.LogError($"Failed to deserialize peers!");
            return;
        }
        logger.Log($"Got {addresses.Count} peers from bootnode");
        peers = addresses;

        // send our address as a broadcast
        byte[] payload = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(address));
        Content ctn = new ContentBuilder()
            .WithHeader("encoding", Encoding.UTF8.WebName)
            .WithPayload(payload)
            .Build();

        logger.Log($"Sending our address to the bootnode...");
        req = new RequestBuilder()
            .WithMethod(RequestMethod.POST)
            .WithRoute(Route.Peers_Join)
            .WithSender(address)
            .WithContent(ctn)
            .WithBroadcast()
            .Build();

        SendRequest(req, bootnode);
        lastPropagations.Add(Convert.ToHexString(req.GetHash()));
    }

    /// <summary>
    /// Spreads a broadcast across the network
    /// </summary>
    /// <param name="req">The request that was sent to this machine</param>
    protected void Broadcast(Request req)
    {
        string hash = Convert.ToHexString(req.GetHash());
        if (lastPropagations.Contains(hash))
        {
            logger.Log($"I have already propagated {hash}!");
            return;
        }
        lastPropagations.Add(hash);

        Parallel.ForEach(peers, peer =>
        {
            logger.Log($"Broadcasting to peer {peer}...");
            SendRequest(req, peer);
        });
    }

    #endregion

    #region Abstract Methods

    /// <summary>
    /// Reads a request from a <see cref="TcpClient"/>
    /// </summary>
    /// <param name="client">The client that sent the request</param>
    /// <returns>The request object</returns>
    protected abstract Request ReadRequest(Address address);

    /// <summary>
    /// Reads a response sent from a <see cref="TcpClient"/>
    /// </summary>
    /// <param name="client">The client that received the Request and sent the Response</param>
    /// <returns></returns>
    protected abstract Response ReadResponse(Address address);

    /// <summary>
    /// Sends a request to a <see cref="TcpClient"/>
    /// </summary>
    /// <param name="request"></param>
    /// <param name="client"></param>
    protected abstract void SendRequest(Request request, Address address);

    /// <summary>
    /// Sends a response to a <see cref="TcpClient"/>
    /// </summary>
    /// <param name="response">The response that will be sent</param>
    /// <param name="client">The client that made the request and will receive the response</param>
    protected abstract void SendResponse(Response response, Address address);

    /// <summary>
    /// Performs the logic for a request. This is run in the internal thread of the node.
    /// </summary>
    /// <param name="request">The Request that was sent</param>
    /// <returns>The response object</returns>
    protected abstract Response Process(Request request);

    protected abstract void ThreadMain();

    #endregion
}
