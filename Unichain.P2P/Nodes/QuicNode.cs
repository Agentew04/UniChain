using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Quic;
using System.Net.Sockets;
using System.Runtime.Versioning;
using Unichain.P2P.Packets;

namespace Unichain.P2P.Nodes; 

/// <summary>
/// A node that used the QUIC protocol to communicate with other nodes
/// </summary>
[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
[SupportedOSPlatform("macOS")]
public class QuicNode : Node {

    #region Variables

    /// <summary>
    /// The logger that manages the printing of messages to the console
    /// </summary>
    private readonly Logger logger;

    /// <summary>
    /// The listener in the QUIC protocol
    /// </summary>
    private readonly QuicListener quicListener;

    #endregion

    /// <summary>
    /// Initialized a new instance of the <see cref="QuicNode"/> class
    /// </summary>
    /// <param name="port">The port that will be used</param>
    /// <exception cref="NotSupportedException">Thrown when the platform doesn't support QUIC</exception>
    public QuicNode(int port) : base(port) {
        if(!QuicListener.IsSupported || !QuicConnection.IsSupported) {
            throw new NotSupportedException("QUIC is not supported on this platform. If running on linux, check if" +
                "libmsquic is installed and check if TLS 1.3 is supported");
        }

        logger = new Logger(nameof(QuicNode) + " " + port.ToString());

    }

    public override void Start(Address? bootnode) {
        quicListener = await QuicListener.ListenAsync(new QuicListenerOptions() {
            ListenEndPoint = new IPEndPoint(IPAddress.Any, address.Port)
        }, cancellationTokenSource.Token);
        base.Start(bootnode);
    }


    protected override Response Process(Request request) {
        throw new NotImplementedException();
    }

    protected override Request ReadRequest(Address address) {
        throw new NotImplementedException();
    }

    protected override Response ReadResponse(Address address) {
        throw new NotImplementedException();
    }

    protected override void SendRequest(Request request, Address address) {
        throw new NotImplementedException();
    }

    protected override void SendResponse(Response response, Address address) {
        throw new NotImplementedException();
    }

    protected override void ThreadMain() => ThreadMainAsync().Wait();

    private async Task ThreadMainAsync() {
        logger.Log($"Listening...");

        // the listen loop
        while (!cancellationTokenSource.IsCancellationRequested) {
            


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
}
