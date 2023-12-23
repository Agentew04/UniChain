using System.Security.Cryptography;
using System.Text;
using Unichain.P2P;

Console.Write("Port: ");
int port = int.Parse(Console.ReadLine() ?? "");

var node = new UnichainNode(port);

Console.Write("Bootnode (ip:port): ");
string bootnode = Console.ReadLine() ?? "";
if (bootnode != "null") {
    var split = bootnode.Split(':');
    var ip = split[0];
    port = int.Parse(split[1]);
    await node.Start(new Address(ip, port));
} else {
    await node.Start(null);
}