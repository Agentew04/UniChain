using Unichain.Core;

namespace Unichain.CLI.Commands
{
    internal class Mine
    {
        internal static int Exec(string[] args, string path)
        {
            var bc = Utils.ParseBlockchain(path);
            if (bc == null) return 4;

            var isaddrfound = Utils.TryGetArgument(args, new()
            {
                Name = "address",
                Simplified = "a"
            }, out string mineraddress);
            if (!isaddrfound)
            {
                Utils.Print("Please provide a address to receive the miner reward!");
                return 1; //bad command
            }
            Utils.Print($"Mining with this address: {mineraddress}");
            bc.MinePendingTransactions(new Address(mineraddress));
            Utils.Print($"Mined sucessfully! Received {bc.Reward} tokens");

            //save chain
            Utils.SaveBlockChain(path, bc);
            return 0;
        }
    }
}
