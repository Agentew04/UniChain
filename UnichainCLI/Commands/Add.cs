using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unichain.Core;

namespace Unichain.CLI.Commands
{
    internal class Add
    {
        internal readonly static List<string> ValidTypes = new()
        {
            "transaction",
            "nftmint",
            "nfttransfer",
            "nftburn",
            "poolopen",
            "poolvote",
            "msgsenduser",
            "msgsendgroup",
            "docsubmit"
        };

        internal static void Exec(string[] args, string path)
        {
            if (!Utils.TryGetArgument(args,new("type","t"), out string typeStr))
            {
                Utils.Print("You need to specify a type to add to the chain! Type Unichain -h for more help");
                return;
            }
            EventType type = EventType.FromCLIString(typeStr);
        }
    }
}
