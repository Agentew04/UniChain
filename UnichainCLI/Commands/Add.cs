using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unichain.Core;
using Unichain.Events;

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

        internal static int Exec(string[] args, string path)
        {
            static T instance<T>()
            {
                return default(T);
            }
            if (!Utils.TryGetArgument(args,new("type","t"), out string typeStr))
            {
                Utils.Print("You need to specify a type to add to the chain! Type Unichain -h for more help");
                return 1;
            }
            if(!ValidTypes.Contains(typeStr))
            {
                Utils.Print("Invalid event type! Type Unichain -h for more help");
                return 1;
            }
            if(!Utils.TryGetArgument(args,new("user","u"),out string privkey))
            {
                //Utils.Print()
                return 3;
            }
            User user = new(privkey);
            EventType eventType = EventType.FromCLIString(typeStr);
            
            switch (eventType)
            {
                case var _ when eventType == EventType.Transaction:

                    //Transaction tx = new()
                    break;
            }
        }
    }
}
