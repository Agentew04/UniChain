using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Text;

namespace RodrigoChain{
    public class NFTBurn : BaseBlockChainEvent
    {
        #region Vars

        /// <summary>
        /// The unique id for this NFT
        /// </summary>
        /// <value></value>
        public Guid NFTId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of a NFT Burn
        /// </summary>
        /// <param name="user">The user executing the action</param>
        /// <param name="nftId">The unique id of the NFT to be burned</param>
        public NFTBurn(User user, Guid nftId) : base(EventType.NFTBurn,user)
        {
            ActionOwner=user;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            NFTId = nftId;
        }

        #endregion
    
        #region Public Methods
        
        public override void SignEvent(User user)
        {
            //TODO: check if the user is the owner of the NFT
            // if (user != this.FromAddress)
            // {
            //     throw new Exception("Invalid key");
            // }

            var HashTransaction = CalculateHash();
            var signature = user.SignMessage(HashTransaction);
            Signature = signature;
        }

        /// <summary>
        /// Checks if the current transaction is valid
        /// </summary>
        /// <returns>A boolean representing the result</returns>
        public bool IsValid(Blockchain blockchain)
        {
            //check addresses and amount

            //TODO: Check if the NFT exists really
            //TODO: Check if the NFT is owned by the user
            //if (blockchain.GetTokenOrigin(this.TokenId).HasValue == false) { return false; }
            //if (blockchain.GetTokenOrigin(this.TokenId).Value.TokenId != TokenId) { return false; }
            if (this.Signature == null) { return false; }
            if(NFTId==Guid.Empty || NFTId == new Guid()) { return false; }
            //check signature
            if (!VerifySignature()) { return false; }

            return true;
        }

        public override bool IsValid()
        {
            throw new NotImplementedException();
        }

        public override string CalculateHash()
        {
            var sha = new Sha3Digest(512);
            byte[] input2 = Encoding.ASCII.GetBytes($"{NFTId}-{Timestamp}");

            sha.BlockUpdate(input2, 0, input2.Length);
            byte[] result = new byte[64];
            sha.DoFinal(result, 0);

            string hash = BitConverter.ToString(result);
            return hash.Replace("-", "").ToLowerInvariant();
        }

        #endregion
    }
}