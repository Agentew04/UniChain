using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unichain.Core;

namespace Unichain.CLI.Commands
{
    internal class Mine
    {
        internal static void Exec(string[] args, string path)
        {
            var bc = Utils.ParseBlockchain(path);
            var isaddrfound = Utils.TryGetArgument(args, new()
            {
                Name = "address",
                Simplified = "a"
            },out string mineraddress);
            if (!isaddrfound)
            {
                Utils.Print("Please provide a address to receive the miner reward!");
                Environment.Exit(5);
                return;
            }
            Utils.Print($"Mining with this address: {mineraddress}");
            bc.MinePendingTransactions(new Address(mineraddress));
            Utils.Print($"Mined sucessfully! Received {bc.Reward} tokens");

            //save chain
            Utils.SaveBlockChain(path, bc);
            Environment.Exit(0);
        }
    }
}
