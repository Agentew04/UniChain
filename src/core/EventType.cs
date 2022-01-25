using Ardalis.SmartEnum;

namespace Unichain.Core
{
    public class EventType : SmartEnum<EventType>
    {
        public static readonly EventType Transaction = new("Transaction", 0);
        public static readonly EventType NFTTransfer = new("NFTTransfer", 1);
        public static readonly EventType NFTBurn = new("NFTBurn", 2);
        public static readonly EventType NFTMint = new("NFTMint", 3);
        public static readonly EventType PoolOpen = new("PoolOpen", 4);
        public static readonly EventType PoolVote = new("PoolVote", 5);
        public static readonly EventType MessageSendUser = new("MessageSendUser", 6);
        public static readonly EventType MessageSendGroup = new("MessageSendGroup", 7);
        public static readonly EventType DocumentSubmit = new("DocumentSubmit",  8);



        private EventType(string name, int value) : base(name, value)
        {
        }

        public static EventType FromCLIString(string input) => input switch
        {
            "transaction" => Transaction,
            "nftmint" => NFTMint,
            "nfttransfer" => NFTTransfer,
            "nftburn" => NFTBurn,
            "poolopen" => PoolOpen,
            "poolvote" => PoolVote,
            "msgsenduser" => MessageSendUser,
            "msgsendgroup" => MessageSendGroup,
            "docsubmit" => DocumentSubmit,
            _ => null
        };

    }
}
        //Transaction,
        //NFTTransfer,
        //NFTBurn,
        //NFTMint,
        //PoolOpen,
        //PoolVote,
        //PoolClose,
        //DocumentSubmit,
        //MessageSendUser,
        //MessageSendGroup,
