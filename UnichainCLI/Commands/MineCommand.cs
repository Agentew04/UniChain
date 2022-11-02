using System;
using Unichain.Core;
using Unichain.Parsing;

namespace Unichain.CLI.Commands;

internal class MineCommand : ICommand {

    public string Name { get; set; } = "mine";
    public List<Flag> RequiredFlags { get; set; } = new List<Flag>() {
        { new("file", "f", true) },
        { new("address", "a", true) }
    };
    public List<Flag> OptionalFlags { get; set; } = new List<Flag>();

    public ReturnCode Invoke(IEnumerable<Flag> flags) {
        string filePath = RequiredFlags.Where(x => x.Full == "file").First().Value ?? "";
        string address = RequiredFlags.Where(x => x.Full == "address").First().Value ?? "";

        filePath = Utils.SanitizePath(filePath, "unichain", ".chain");
        if (!File.Exists(filePath)) {
            Console.WriteLine("Blockchain not found!");
            return ReturnCode.BlockChainNotFound;
        }

        if(!PublicKey.IsAddressValid(address)) {
            Console.WriteLine("Invalid address!");
            return ReturnCode.InvalidAddress;
        }

        var parser = new BlockchainParser();
        Blockchain bc;
        using (var fs = File.OpenRead(filePath)) {
            bc = parser.DeserializeBlockchain(fs);
            if(bc is null) {
                Console.WriteLine("Bad blockchain");
                return ReturnCode.InvalidBlockchain;
            }
        }

        bc.MinePendingTransactions(address);

        // save blockchain
        using (var fs = File.OpenWrite(filePath)) {
            var ms = parser.SerializeBlockchain(bc);
            ms.CopyTo(fs);
        }
        
        return ReturnCode.Success;
    }

    public void Help() {
        Console.WriteLine(@"
Possible flags for 'mine' sub-command:
  -a  --address => Sets the address that will receive the coins
  -f  --file    => Path to the json file that the blockchain is stored");
    }
    
    //internal static int Exec(string[] args, string path)
    //{
    //    var bc = Utils.ParseBlockchain(path);
    //    if (bc == null) return 4;

        //var isaddrfound = Utils.TryGetArgument(args, new()
        //{
        //    Full = "address",
        //    Simplified = "a"
        //}, out string mineraddress);
        //if (!isaddrfound)
        //{
        //    Utils.Print("Please provide a address to receive the miner reward!");
        //    return 1; //bad command
        //}
        //Utils.Print($"Mining with this address: {mineraddress}");
        //bc.MinePendingTransactions(mineraddress);
    //    Utils.Print($"Mined sucessfully! Received {bc.Reward} tokens");

    //    //save chain
    //    Utils.SaveBlockChain(path, bc);
    //    return 0;
    //}
}
