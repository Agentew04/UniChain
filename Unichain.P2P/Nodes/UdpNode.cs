using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Unichain.P2P.Packets;

namespace Unichain.P2P.Nodes {
    public class UdpNode : Node {

        /// <summary>
        /// The client that will receive and send packets
        /// </summary>
        private UdpClient? udpClient;

        /// <summary>
        /// Logger to log messages to the console
        /// </summary>
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Creates a new instance of a node using UDP as the communication protocol
        /// </summary>
        /// <param name="port">The port that will be used</param>
        public UdpNode(int port) : base(port) { }

        public override void Start(Address? bootnode) {
            udpClient = new UdpClient(address.Port);
            base.Start(bootnode);
        }

        public override void Stop() {
            base.Stop();
            if(udpClient is not null) {
                udpClient.Close();
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

        protected override Request ReadRequest(Address address) {
            IPEndPoint sender = new(this.address.Normalize(address), address.Port);
            byte[] buffer = udpClient!.Receive(ref sender);
            using MemoryStream ms = new(buffer);
            return Request.Read(ms);
        }

        protected override Response ReadResponse(Address address) {
            IPEndPoint sender = new(this.address.Normalize(address), address.Port);
            byte[] buffer = udpClient!.Receive(ref sender);
            using MemoryStream ms = new(buffer);
            return Response.Read(ms);
        }

        protected override void SendRequest(Request request, Address address) {
            IPEndPoint receiver = new(this.address.Normalize(address), address.Port);
            using MemoryStream ms = new();
            request.Write(ms);
            udpClient!.Send(ms.ToArray(), (int)ms.Length, receiver);
        }

        protected override void SendResponse(Response response, Address address) {
            IPEndPoint receiver = new(this.address.Normalize(address), address.Port);
            using MemoryStream ms = new();
            response.Write(ms);
            udpClient!.Send(ms.ToArray(), (int)ms.Length, receiver);
        }

        protected override void ThreadMain() => ThreadMainAsync().Wait();
    
        private async Task ThreadMainAsync() {
            logger.Info($"Listening...");

            // the listen loop
            while (!cancellationTokenSource.IsCancellationRequested) {
                logger.Debug($"Waiting for connection...");
                UdpReceiveResult result;
                try {
                    result = await udpClient!.ReceiveAsync(cancellationTokenSource.Token);
                } catch (OperationCanceledException){
                    logger.Warn("Cancelling packet receiving.");
                    continue;
                }
                logger.Debug("Received connection from {endpoint}. Buffer size: {length} bytes",
                    result.RemoteEndPoint,
                    result.Buffer.Length);

                using MemoryStream inStream = new(result.Buffer);
                using MemoryStream outStream = new();

                // Read the request
                logger.Debug("Reading request...");
                Request request = Request.Read(inStream);

                // Process the request
                logger.Debug("Processing request...");
                Response response = Process(request);

                // Send the response or broadcast
                if (!request.IsBroadcast) {
                    logger.Debug("Sending response...");
                    response.Write(outStream);
                    udpClient.Send(outStream.ToArray(), (int)outStream.Length, result.RemoteEndPoint);
                } else {
                    logger.Debug("Broadcasting request...");
                    Broadcast(request);
                }

                // Close the connection
                logger.Info("Closed connection with {addresss}", address.Normalize(request.Sender));

                // Log in files the response and request
                string now = DateTime.Now.ToString().Replace(':', '-').Replace('/', '-');
                using (FileStream fs = File.Create($"request-{now}.bin")) {
                    inStream.Seek(0, SeekOrigin.Begin);
                    inStream.CopyTo(fs);
                }
                using (FileStream fs = File.Create($"response-{now}.bin")) {
                    outStream.Seek(0, SeekOrigin.Begin);
                    outStream.CopyTo(fs);
                }

            }
        }
    }
}
