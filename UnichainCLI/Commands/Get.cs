using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unichain.Core;

namespace Unichain.CLI.Commands
{
    internal class Get
    {
        internal static void Exec(string[] args, string path)
        {
            var bc = Utils.ParseBlockchain(path);
            bool isbalance = args.Contains("--balance");
            bool isnft = args.Contains("--nft");
            bool hasaddress = Utils.TryGetArgument(args, new("address", "a"), out string addressStr);
            Address address = new(addressStr);
            if (hasaddress)
            {
                if (isbalance)
                {
                    Utils.Print($"{bc.GetBalance(address)}");
                    Environment.Exit(0);
                }
                if (isnft)
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
