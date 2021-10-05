using NBitcoin;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodrigoChain
{
    public class NFTTransaction : BaseBlockChainEvent
    {
        #region Variables
        //in hash
        /// <summary>
        /// The Address that the coins will withdrawed
        /// </summary>
        public Address FromAddress { get; set; }


        /// <summary>
        /// The receiver of the coins
        /// </summary>
        public Address ToAddress { get; set; }


        /// <summary>
        /// The <see cref="TokenId"/> of the Token being transferred
        /// </summary>
        public Guid TokenId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new Token Transaction. 
        /// </summary>
        /// <param name="fromAddress">Same of the <see cref="PubKey"/></param>
        /// <param name="toAddress">The Address of the receiver</param>
        /// <param name="tokenId">The Id of the token being transferred</param>
        public NFTTransaction(User user, Address toAddress, Guid tokenId) : base(EventType.NFTTransaction,user)
        {
            ActionOwner = user;
            FromAddress = user.Address;
            ToAddress = toAddress;
            TokenId = tokenId;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
        }

        #endregion

        #region Methods
        
        public override void SignEvent(User user)
        {
            //check is the owner making the transaction
            if (user != this.FromAddress)
            {
                throw new Exception("Invalid key");
            }

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
            if (blockchain.GetTokenOrigin(this.TokenId).HasValue == false) { return false; }
            if (blockchain.GetTokenOrigin(this.TokenId).Value.TokenId != TokenId) { return false; }
            if (this.Signature == null) { return false; }
            if (this.FromAddress.IsNull() || this.ToAddress.IsNull() || TokenId == new Guid()) { return false; }
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
            byte[] input2 = Encoding.ASCII.GetBytes($"{FromAddress}-{ToAddress}-{TokenId}-{Timestamp}");

            sha.BlockUpdate(input2, 0, input2.Length);
            byte[] result = new byte[64];
            sha.DoFinal(result, 0);

            string hash = BitConverter.ToString(result);
            return hash.Replace("-", "").ToLowerInvariant();
        }

        #endregion
    }
}
