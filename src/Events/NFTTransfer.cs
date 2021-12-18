using NBitcoin;
using RodrigoChain.Core;
using System;
using System.Security.Cryptography;
using System.Text;

namespace RodrigoChain.Events
{
    public class NFTTransfer : BaseBlockChainEvent
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
        /// The <see cref="NFTId"/> of the Token being transferred
        /// </summary>
        public Guid NFTId { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new Token Transaction. 
        /// </summary>
        /// <param name="fromAddress">Same of the <see cref="PubKey"/></param>
        /// <param name="toAddress">The Address of the receiver</param>
        /// <param name="tokenId">The Id of the token being transferred</param>
        public NFTTransfer(User user, Address toAddress, Guid tokenId) : base(EventType.NFTTransfer,user)
        {
            ActionOwner = user;
            FromAddress = user.Address;
            ToAddress = toAddress;
            NFTId = tokenId;
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
        public override bool IsValid(Blockchain blockchain)
        {
            //check addresses and amount

            //TODO: check if the NFT exists
            //TODO: check if the NFT is owned by the sender
            //if (blockchain.GetTokenOrigin(this.TokenId).HasValue == false) { return false; }
            //if (blockchain.GetTokenOrigin(this.TokenId).Value.TokenId != TokenId) { return false; }
            if (this.Signature == null) { return false; }
            if(NFTId==Guid.Empty || NFTId == new Guid()) { return false; }
            if (this.FromAddress.IsNull() || this.ToAddress.IsNull()) { return false; }
            //check signature
            if (!VerifySignature()) { return false; }

            return true;
        }

        public override string CalculateHash()
        {
            //calculate sha512 hash using nftid, timestamp and burneraddress
            var bytes = System.Text.Encoding.UTF8.GetBytes($"{FromAddress}-{ToAddress}-{NFTId}-{Timestamp}");
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
