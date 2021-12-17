using System;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Crypto.Digests;
using RodrigoChain.Exceptions;
using RodrigoChain;
using RodrigoChain.Core;

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
            var sha = new Sha3Digest(512);
            byte[] input2 = Encoding.ASCII.GetBytes($"{PoolId}-{VoteIndex}-{VoterAddress}");

            sha.BlockUpdate(input2, 0, input2.Length);
            byte[] result = new byte[64];
            sha.DoFinal(result, 0);

            string hash = BitConverter.ToString(result);
            return hash.Replace("-", "").ToLowerInvariant();
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