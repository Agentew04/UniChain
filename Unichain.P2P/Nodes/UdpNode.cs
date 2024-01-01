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
        private UdpClient udpClient;

        /// <summary>
        /// Logger to log messages to the console
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        /// Creates a new instance of a node using UDP as the communication protocol
        /// </summary>
        /// <param name="port">The port that will be used</param>
        public UdpNode(int port) : base(port) { 
            logger = new Logger(nameof(UdpNode) + " " + port.ToString());
        }

        public override void Start(Address? bootnode) {
            udpClient = new UdpClient(address.Port);
            base.Start(bootnode);
        }

        public override void Stop() {
            base.Stop();
            udpClient.Close();
        }

        protected override Response Process(Request request) {
            logger.Log($"Received {request.GetType().Name} from {address.Normalize(request.Sender)}");

            return (string)request.Route switch {
                "/ping" => Response.ok,
                "/peers" => Response.ok,
                "/peers/join" => Response.ok,
                _ => new ResponseBuilder()
                                        .WithStatusCode(StatusCode.NotFound)
                                        .Build(),
            };
        }

        protected override Request ReadRequest(Address address) {
            IPEndPoint sender = new(this.address.Normalize(address), address.Port);
            byte[] buffer = udpClient.Receive(ref sender);
            using MemoryStream ms = new(buffer);
            return Request.Read(ms);
        }

        protected override Response ReadResponse(Address address) {
            IPEndPoint sender = new(this.address.Normalize(address), address.Port);
            byte[] buffer = udpClient.Receive(ref sender);
            using MemoryStream ms = new(buffer);
            return Response.Read(ms);
        }

        protected override void SendRequest(Request request, Address address) {
            IPEndPoint receiver = new(this.address.Normalize(address), address.Port);
            using MemoryStream ms = new();
            request.Write(ms);
            udpClient.Send(ms.ToArray(), (int)ms.Length, receiver);
        }

        protected override void SendResponse(Response response, Address address) {
            IPEndPoint receiver = new(this.address.Normalize(address), address.Port);
            using MemoryStream ms = new();
            response.Write(ms);
            udpClient.Send(ms.ToArray(), (int)ms.Length, receiver);
        }

        protected override void ThreadMain() => ThreadMainAsync().Wait();
    
        private async Task ThreadMainAsync() {
            logger.Log($"Listening...");

            // the listen loop
            while (!cancellationTokenSource.IsCancellationRequested) {
                logger.Log($"Waiting for connection...");
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                logger.Log($"Received connection from {result.RemoteEndPoint}. Buffer size: {result.Buffer.Length} bytes");

                using MemoryStream inStream = new(result.Buffer);
                using MemoryStream outStream = new();

                // Read the request
                logger.Log($"Reading request...");
                Request request = Request.Read(inStream);

                // Process the request
                logger.Log($"Processing request...");
                Response response = Process(request);

                // Send the response or broadcast
                if (!request.IsBroadcast) {
                    logger.Log($"Sending response...");
                    response.Write(outStream);
                    udpClient.Send(outStream.ToArray(), (int)outStream.Length, result.RemoteEndPoint);
                } else {
                    logger.Log("Broadcasting request...");
                    Broadcast(request);
                }

                // Close the connection
                logger.Log($"Closed connection with {address.Normalize(request.Sender)}");

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
