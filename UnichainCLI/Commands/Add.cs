using Newtonsoft.Json;
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
            Blockchain? blockchain = Utils.ParseBlockchain(path);
            if (!Utils.TryGetArgument(args, new("type", "t"), out string typeStr))
            {
                Utils.Print("You need to specify a type to add to the chain! Type Unichain -h for more help");
                return 1;
            }
            if (!ValidTypes.Contains(typeStr))
            {
                Utils.Print("Invalid event type! Type Unichain -h for more help");
                return 1;
            }
            if (!Utils.TryGetArgument(args, new("user", "u"), out string privkey))
            {
                Utils.Print("Missing private key! Include one with --user or -u");
                return 3;
            }
            if (blockchain == null) return 4;

            User user;
            BaseBlockChainEvent? @event;

            try
            {
                user = new(new PrivateKey(Convert.FromHexString(privkey)));
                @event = typeStr switch
                {
                    "transaction" => CreateTransaction(args, user),
                    "nftmint" => CreatNFTMint(args, user),
                    "nfttransfer" => CreateNFTTransfer(args, user),
                    "nftburn" => CreateNFTBurn(args, user),
                    "poolopen" => CreatePoolOpen(args, user),
                    "poolvote" => CreatePoolVote(args, user, blockchain),
                    _ => throw new NotImplementedException()
                };
            }
            catch (Exception) { return 7; }
            if (@event == null) return 1;

            blockchain?.AddEvent(@event);
            return 0;

        }

        private static bool TryGetMeta<T>(string path, out T? output)
        {
            try
            {
                var json = File.ReadAllText(path);
                output = JsonConvert.DeserializeObject<T>(json);
                return true;
            }
            catch (Exception ex)
            {
                Utils.Print(ex.Message);
                output = default;
                return false;
            }
        }

        private static ITransaction? CreateTransaction(string[] args, User user)
        {
            if (!Utils.TryGetArgument(args, new("receiver", "r"), out string receiverAddress)) return null;
            if (!Utils.TryGetArgument(args, new("amount", ""), out string amountString)) return null;

            double amount = Convert.ToDouble(amountString);

            ITransaction tx = new(user, receiverAddress, amount);
            tx.SignEvent(user);

            return tx;
        }

        private static NFTMint? CreatNFTMint(string[] args, User user)
        {
            if (!Utils.TryGetArgument(args, new("meta", "m"), out string metaPath)) return null;
            if (!TryGetMeta<NFTMetadata>(metaPath, out var meta)) return null;

            NFTMint tx = new(user, meta);
            tx.SignEvent(user);

            return tx;
        }

        private static NFTTransfer? CreateNFTTransfer(string[] args, User user)
        {
            if (!Utils.TryGetArgument(args, new("id", "i"), out string idstr)) return null;
            if (!Utils.TryGetArgument(args, new("receiver", ""), out string receiverAddr)) return null;
            if (!Guid.TryParse(idstr, out Guid id)) return null;
            
            NFTTransfer tx = new(user, receiverAddr, id);
            tx.SignEvent(user);
            return tx;
        }

        private static NFTBurn? CreateNFTBurn(string[] args, User user)
        {
            if (!Utils.TryGetArgument(args, new("id", "id"), out string idstr)) return null;
            if (!Guid.TryParse(idstr, out Guid id)) return null;

            NFTBurn tx = new(user, id);
            tx.SignEvent(user);
            return tx;
        }

        private static PoolOpen? CreatePoolOpen(string[] args, User user)
        {
            if (!Utils.TryGetArgument(args, new("meta", "m"), out string metaPath)) return null;
            if (!TryGetMeta<PoolMetadata>(metaPath, out var meta)) return null;

            PoolOpen tx = new(user, meta);
            tx.SignEvent(user);
            return tx;
        }

        private static PoolVote? CreatePoolVote(string[] args, User user, Blockchain blockchain)
        {
            if (!Utils.TryGetArgument(args, new("id", "i"), out string idstr)) return null;
            if (!Utils.TryGetArgument(args, new("vote", "v"), out double voteindex)) return null;
            if (!Guid.TryParse(idstr, out Guid id)) return null;

            PoolVote tx = new(user, id, (int)voteindex, blockchain);
            tx.SignEvent(user);
            return tx;
        }
    }

}
