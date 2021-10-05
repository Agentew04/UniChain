using System;
using System.Text;
using System.Text.Json;
using Org.BouncyCastle.Crypto.Digests;
using RodrigoChain.Exceptions;

namespace RodrigoChain{
    public class VoteCast : BaseBlockChainEvent{
        
        #region Variables
             
        public Guid VoteId { get; set; }

        public int OptionPickedIndex { get; set; }

        public Address VoterAddress { get; set; }

        

        #endregion

        #region Constructor

        public VoteCast(User user, Guid voteId, int optionPickedIndex) :base(EventType.VoteCast,user)
        {
            ActionOwner=user;
        }

        #endregion

        #region Methods

        public override string CalculateHash()
        {
            var sha = new Sha3Digest(512);
            byte[] input2 = Encoding.ASCII.GetBytes($"{VoteId}-{OptionPickedIndex}-{VoterAddress}");

            sha.BlockUpdate(input2, 0, input2.Length);
            byte[] result = new byte[64];
            sha.DoFinal(result, 0);

            string hash = BitConverter.ToString(result);
            return hash.Replace("-", "").ToLowerInvariant();
        }

        public override bool IsValid()
        {
            if(Signature==null)return false;
            if(OptionPickedIndex<0)return false;
            if(VoterAddress.IsNull())return false;
            if(Guid.Empty.Equals(VoteId))return false;
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