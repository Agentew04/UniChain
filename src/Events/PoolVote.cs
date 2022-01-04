using System;
using System.Text;
using Unichain.Exceptions;
using Unichain.Core;
using System.Security.Cryptography;

namespace Unichain.Events
{
    public class PoolVote : BaseBlockChainEvent, ISubEventable<Transaction> {
        
        #region Variables
             
        public Guid PoolId { get; set; }

        public int VoteIndex { get; set; }

        public Address VoterAddress { get; set; }

        public bool HasFee { get; set; }
        public Transaction SubEvent { get ; set ; }

        #endregion

        #region Constructor

        public PoolVote(User user, Guid poolId, int voteIndex, Blockchain blockchain) :base(EventType.PoolVote,user)
        {
            EventType=EventType.PoolVote;
            ActionOwner=user;
            VoterAddress=user.Address;
            PoolId=poolId;
            VoteIndex=voteIndex;
            Timestamp=DateTime.UtcNow.Ticks;
            PoolOpen poolOpen = blockchain.GetPoolById(PoolId);
            if (poolOpen != null && poolOpen.Metadata.Fee > 0)
            {
                HasFee = true;
                Transaction tx = new(user, poolOpen.Owner, poolOpen.Metadata.Fee);
                tx.SignEvent(user);
                SubEvent = tx;
            }
            else
            {
                SubEvent = null;
                HasFee = false;
            }
        }

        #endregion

        #region Methods

        public override string CalculateHash()
        {
            //calculate sha512 hash using nftid, timestamp and burneraddress
            var bytes = Encoding.UTF8.GetBytes($"{PoolId}-{VoteIndex}-{VoterAddress}-{SubEvent}");
            using var hash = SHA512.Create();
            var hashedInputBytes = hash.ComputeHash(bytes);

            // Convert to text
            // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
            var hashedInputStringBuilder = new StringBuilder(128);
            foreach (var b in hashedInputBytes)
                hashedInputStringBuilder.Append(b.ToString("X2"));
            return hashedInputStringBuilder.ToString();
        }

        public override bool IsValid(Blockchain blockchain)
        {
            // Check if the pool exists

            if (blockchain.GetPoolById(PoolId) == null) return false;
            if (Signature==null)return false;
            if (VoteIndex<0)return false;
            if (VoterAddress.IsNull())return false;
            if (Guid.Empty.Equals(PoolId))return false;
            if (!VerifySignature())return false;
            if (HasFee)
            {
                if (FeeTransaction == null) return false;
                if (!FeeTransaction.IsValid(blockchain)) return false;
            }
            return true;
        }

        public override void SignEvent(User user)
        {
            if ( user != this.VoterAddress) throw new InvalidKeyException();

            var hashTransaction = CalculateHash();
            var signature = user.SignMessage(hashTransaction);
            Signature = signature;
        }

        #endregion
    }
}