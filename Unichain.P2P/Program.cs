using NLog;
using Unichain.P2P;
using Unichain.P2P.Nodes;


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
