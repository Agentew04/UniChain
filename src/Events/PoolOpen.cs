using System;
using System.Text;
using System.Text.Json;
using Unichain.Exceptions;
using Unichain.Core;
using System.Security.Cryptography;

namespace Unichain.Events
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

            //calculate sha512 hash using nftid, timestamp and burneraddress
            var bytes = System.Text.Encoding.UTF8.GetBytes($"{Owner.ToString()}-{PoolId.ToString()}-{json(Metadata)}");
            using (var hash = SHA512.Create())
            {
                var hashedInputBytes = hash.ComputeHash(bytes);

                // Convert to text
                // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
                var hashedInputStringBuilder = new StringBuilder(128);
                foreach (var b in hashedInputBytes)
                    hashedInputStringBuilder.Append(b.ToString("X2"));
                return hashedInputStringBuilder.ToString();
            }
        }

        #endregion
    }
}