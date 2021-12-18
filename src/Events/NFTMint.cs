using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RodrigoChain.Core;
using System;
using System.Security.Cryptography;
using System.Text;

namespace RodrigoChain.Events
{
    public class NFTMint : BaseBlockChainEvent
    {
        #region Variables

        /// <summary>
        /// The unique Id for this Token
        /// </summary>
        public Guid NFTId { get; }

        /// <summary>
        /// The Address of the Owner of this Token
        /// </summary>
        public Address Owner { get; set; }

        /// <summary>
        /// The custom metadata for this Token
        /// </summary>
        public NFTMetadata NFTMetadata {get;set;}

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new token to be published on the blockchain
        /// </summary>
        /// <param name="owner">The first owner for the token</param>
        /// <param name="metadata">The metadata of this token</param>
        public NFTMint(User user, NFTMetadata metadata) : base(EventType.NFTMint,user){
            this.ActionOwner=user;
            this.Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            this.NFTMetadata=metadata;
            this.Owner=user.Address;
            this.NFTId = Guid.NewGuid();
        }

        #endregion

        #region Methods

        public override bool IsValid(Blockchain blockchain)
        {
            //TODO: check if nft already exists
            
            if( Signature == null) { return false; }
            if (Owner.IsNull() || NFTMetadata == null){ return false; }
            if (NFTMetadata.Name == null || NFTMetadata.Description == null){ return false; }
            if (NFTMetadata.ImageUrl == null){ return false; }
            if (!VerifySignature()) { return false; }
            return true;
        }

        /// <summary>
        /// Returns a JObject equivalent to this <see cref="NFTMint"/>, similar to <see cref="Transaction"/>
        /// </summary>
        /// <returns>A JObject that can be converted to string</returns>
        public JObject ToJObject()
        {
            return JObject.FromObject(this);
        }

        /// <summary>
        /// Returns a JSON serialized string
        /// </summary>
        /// <returns>A string in JSON format</returns>
        public override string ToString()
        {
            return this.ToJObject().ToString(Formatting.Indented);
        }

        public override string CalculateHash()
        {
            //calculate sha512 hash using nftid, timestamp and burneraddress
            var bytes = System.Text.Encoding.UTF8.GetBytes($"{this.Owner}-{this.Timestamp}-{this.NFTId}-{this.NFTMetadata}");
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

        public override void SignEvent(User user)
        {
            if (user.Address != this.Owner)
            {
                throw new Exception("Invalid key");
            }

            var hash = CalculateHash();
            var signature = user.SignMessage(hash);
            Signature = signature;
        }

        #endregion
    }
}
