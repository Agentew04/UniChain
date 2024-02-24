using LiteNetLib;
using LiteNetLib.Utils;
using NLog;
using Unichain.P2P;
using Unichain.P2P.Nodes;

#pragma warning disable CS4014 // Como esta chamada não é esperada, a execução do método atual continua antes de a chamada ser concluída
Task.Delay(5000).ContinueWith(t => {
    EventBasedNetListener listener = new EventBasedNetListener();
    NetManager client = new NetManager(listener);
    client.Start();
    client.Connect("localhost" /* host ip or name */, 9050 /* port */, "SomeConnectionKey" /* text key or NetDataWriter */);
    listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
    {
        Console.WriteLine("We got: {0}", dataReader.GetString(100 /* max length of string */));
        dataReader.Recycle();
    };

    while (!Console.KeyAvailable) {
        client.PollEvents();
        Thread.Sleep(15);
    }

    client.Stop();
});
#pragma warning restore CS4014 // Como esta chamada não é esperada, a execução do método atual continua antes de a chamada ser concluída
EventBasedNetListener listener = new EventBasedNetListener();
NetManager server = new NetManager(listener);
server.Start(9050 /* port */);

listener.ConnectionRequestEvent += request => {
    if (server.ConnectedPeersCount < 10 /* max connections */)
        request.AcceptIfKey("SomeConnectionKey");
    else
        request.Reject();
};

listener.PeerConnectedEvent += peer => {
    Console.WriteLine("We got connection: {0}", peer);  // Show peer ip
    NetDataWriter writer = new NetDataWriter();         // Create writer class
    writer.Put("Hello client!");                        // Put some string
    peer.Send(writer, DeliveryMethod.ReliableOrdered);  // Send with reliability
};

while (!Console.KeyAvailable) {
    server.PollEvents();
    Thread.Sleep(15);
}
server.Stop();


return;
List<UnichainNode> nodes = [];

int bootnodePort = 1234;
const int nodeCount = 1;

var bootnode = new UnichainNode(bootnodePort);
for(int i= 1; i <= nodeCount; i++) {
    var node = new UnichainNode(bootnodePort + i);
    nodes.Add(node);
}

bootnode.Start(null);

Parallel.ForEach(nodes, node => {
    node.Start(bootnode.Address);
});

// stopping everything
bootnode.Stop();
nodes.ForEach(node => node.Stop());
LogManager.Shutdown();
