using System.Security.Cryptography;
using System.Text;
using Unichain.P2P;

List<UnichainNode> nodes = new();

int bootnodePort = 1234;
const int nodeCount = 1;
var bootnode = new UnichainNode(bootnodePort);
for(int i= 1; i <= nodeCount; i++) {
    var node = new UnichainNode(bootnodePort + i);
    nodes.Add(node);
}

await bootnode.Start(null);

Parallel.ForEach(nodes, async node => {
    await node.Start(new Address("localhost", bootnodePort));
});
await bootnode.Stop();