using System;
using System.Text;
using RodrigoChain.Exceptions;
using RodrigoChain.Core;
using System.Security.Cryptography;

namespace RodrigoChain.Events
{
    public class PoolVote : BaseBlockChainEvent{
        
        #region Variables
             
        public Guid PoolId { get; set; }

        public int VoteIndex { get; set; }

        public Address VoterAddress { get; set; }

        

        #endregion

        #region Constructor

        public PoolVote(User user, Guid poolId, int voteIndex) :base(EventType.PoolVote,user)
        {
            EventType=EventType.PoolVote;
            ActionOwner=user;
            VoterAddress=user.Address;
            PoolId=poolId;
            VoteIndex=voteIndex;
            Timestamp=DateTime.UtcNow.ToFileTimeUtc();
        }

        #endregion

        #region Methods

        public override string CalculateHash()
        {
            //calculate sha512 hash using nftid, timestamp and burneraddress
            var bytes = System.Text.Encoding.UTF8.GetBytes($"{PoolId}-{VoteIndex}-{VoterAddress}");
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

        public override bool IsValid(Blockchain blockchain)
        {
            // Check if the pool exists

            if(Signature==null)return false;
            if(VoteIndex<0)return false;
            if(VoterAddress.IsNull())return false;
            if(Guid.Empty.Equals(PoolId))return false;
            if(!VerifySignature())return false;
            return true;
        }

        public override void SignEvent(User user)
        {
            if ( user != this.VoterAddress)
            {
                throw new InvalidKeyException();
            }

            var hashTransaction = CalculateHash();
            var signature = user.SignMessage(hashTransaction);
            Signature = signature;
        }

        #endregion
    }
}