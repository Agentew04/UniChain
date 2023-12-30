using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Unichain.P2P.Packets;

namespace Unichain.P2P.Nodes;
public abstract class TcpNode : Node
{

    #region Variables

    /// <summary>
    /// Listener to receive messages from other nodes
    /// </summary>
    private readonly TcpListener tcpListener;

    /// <summary>
    /// Logger to log messages to the console
    /// </summary>
    private readonly Logger logger;

    #endregion

    /// <summary>
    /// Initializes variables for the <see cref="TcpNode"/>
    /// </summary>
    /// <param name="port">The port that this node will listen</param>
    protected TcpNode(int port) : base(port)
    {
        tcpListener = new(new IPEndPoint(IPAddress.Any, port));
        logger = new Logger(nameof(TcpNode) + " " + port.ToString());
    }


    #region Protected Methods

    
    protected static Request ReadRequest(TcpClient client) => Request.Read(client.GetStream());

    protected static void SendRequest(Request request, TcpClient client) => request.Write(client.GetStream());

    protected static void SendResponse(Response response, TcpClient client) => response.Write(client.GetStream());

    protected static Response ReadResponse(TcpClient client) => Response.Read(client.GetStream());

    #endregion

    #region Private Methods

    protected override void ThreadMain()
    {
        tcpListener.Start();
        logger.Log($"Listening...");

        // the listen loop
        while (!cancellationTokenSource.IsCancellationRequested)
        {
            TcpClient incoming = tcpListener.AcceptTcpClient();
            NetworkStream inStream = incoming.GetStream();

            // Read the request
            Request request = Request.Read(inStream);

            // Process the request
            Response response = Process(request);

            // Send the response or broadcast
            if (!request.IsBroadcast)
            {
                response.Write(inStream);
            }
            else
            {
                Broadcast(request);
            }

            // Close the connection
            logger.Log($"Closed connection with {((IPEndPoint)incoming.Client.RemoteEndPoint!).Address}");
            incoming.Close();
        }
    }

    #endregion

}
