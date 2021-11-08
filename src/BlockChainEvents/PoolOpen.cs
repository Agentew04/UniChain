using System;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Crypto.Digests;
using RodrigoChain.Exceptions;
using System.Collections.Generic;

namespace RodrigoChain{
    public class PoolOpen : BaseBlockChainEvent
    {
        #region Variables
        public Address Owner { get; set; }
        public string[] Options {get;set;}
        public Guid VoteId {get;set;}
        public Dictionary<string,string> Meta { get; set; }
        public int MinumumAmountToVote { get; set; }

        public PoolOpen(User user, string[] options, Dictionary<string,string> meta) : base(EventType.PoolOpen,user)
        {
            ActionOwner=user;
            Owner = (Address)user;
        }

        public override bool IsValid()
        {
            if( Signature == null) { return false; }
            if (Owner.IsNull() || Meta == null){ return false; }
            if (!VerifySignature()) { return false; }
            return true;
        }

        public override void SignEvent(User user)
        {
            //check is the owner making the transaction
            if (user != this.Owner)
            {
                throw new InvalidKeyException();
            }

            var HashTransaction = CalculateHash();
            var signature = user.SignMessage(HashTransaction);
            Signature = signature;
        }

        public override string CalculateHash()
        {
            string json(object o)=>JsonSerializer.Serialize(o);
            var sha = new Sha3Digest(512);
            byte[] input2 = Encoding.ASCII.GetBytes($"{json(Owner)}-{json(Options)}-{VoteId}-{json(Meta)}-{MinumumAmountToVote}");

            sha.BlockUpdate(input2, 0, input2.Length);
            byte[] result = new byte[64];
            sha.DoFinal(result, 0);

            string hash = BitConverter.ToString(result);
            return hash.Replace("-", "").ToLowerInvariant();
        }

        #endregion
    }
}