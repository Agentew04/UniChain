using Unichain.Core;

namespace Unichain.CLI.Commands
{
    internal class Get
    {
        internal static int Exec(string[] args, string path)
        {
            var bc = Utils.ParseBlockchain(path);
            if (bc == null) return 4; // bad blockchain

            bool isbalance = Utils.HasFlag(args, new("balance", ""));
            bool isnft = Utils.HasFlag(args, new("nft", ""));
            bool hasaddress = Utils.TryGetArgument(args, new("address", "a"), out string address);
            if (hasaddress)
            {
                if (isbalance)
                {
                    Utils.Print($"{bc?.GetBalance(address)}");
                    return 0;
                }
                if (isnft)
                {
                    return 6;
                }
            }
            return 0;
        }
    }
}
