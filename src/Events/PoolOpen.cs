using System;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Crypto.Digests;
using RodrigoChain.Exceptions;
using System.Collections.Generic;
using RodrigoChain.Core;

namespace RodrigoChain.Events
{
    public class PoolOpen : BaseBlockChainEvent
    {
        #region Variables

        public Address Owner { get; set; }
        public Guid PoolId {get;set;}
        public PoolMetadata Metadata { get; set; }

        #endregion

        #region Constructors
        public PoolOpen(User user, PoolMetadata metadata) : base(EventType.PoolOpen,user)
        {
            ActionOwner=user;
            EventType = EventType.PoolOpen;
            Owner = user.Address;
            PoolId = Guid.NewGuid();
            Metadata = metadata;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();

        }
        #endregion

        #region Methods

        public override bool IsValid(Blockchain blockchain)
        {
            if( Signature == null) { return false; }
            if (Owner.IsNull() || Metadata == null){ return false; }
            if (!VerifySignature()) { return false; }
            return true;
        }

        public override void SignEvent(User user)
        {
            //check is the owner making the event
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
            byte[] input2 = Encoding.ASCII.GetBytes($"{Owner.ToString()}-{PoolId.ToString()}-{json(Metadata)}");

            sha.BlockUpdate(input2, 0, input2.Length);
            byte[] result = new byte[64];
            sha.DoFinal(result, 0);

            string hash = BitConverter.ToString(result);
            return hash.Replace("-", "").ToLowerInvariant();
        }

        #endregion
    }
}