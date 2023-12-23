using System.Security.Cryptography;
using System.Text;
using Unichain.P2P;

Console.Write("Port: ");
int port = int.Parse(Console.ReadLine() ?? "");

var node = new Node(port);

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


//new Thread(async () => {
//    while (true) {
//        Console.Write("Message: ");
//        string message = Console.ReadLine() ?? "";
//        SHA256 sha256 = SHA256.Create();
//        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(message));

//        await node.Broadcast(message, Convert.ToHexString(hash));
//    }
//}).Start();