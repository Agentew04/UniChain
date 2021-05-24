using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodrigoCoin_v2
{
    public class TokenCreation : BlockChainEvent
    {
        #region Variables
        /// <summary>
        /// A GUID/UUID for this token, randomly generated
        /// </summary>
        public Guid TokenId { get; }

        /// <summary>
        /// The first owner for this token, must be a valid address otherwise you will not
        /// be able to transfer it to anyone
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// The timestamp of the <see cref="TokenCreation"/>, corresponds to the time the object
        /// was created, not the time it was added to the <seealso cref="Blockchain"/>
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Represents the Signed Hash of this transaction
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// The metadata for this token, defines how it looks like
        /// </summary>
        public TokenMetadata Metadata { get; }
        #endregion

        #region constructors

        /// <summary>
        /// Creates a new token to be published on the blockchain
        /// </summary>
        /// <param name="owner">The first owner for the token</param>
        /// <param name="metadata">The metadata of this token</param>
        public TokenCreation(string owner, TokenMetadata metadata)
        {
            this.TokenId = Guid.NewGuid();
            this.Owner = owner;
            this.Metadata = metadata;
            this.Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            EventType = EventType.TokenCreation;
        }

        #endregion

        /// <summary>
        /// Checks if this token is valid
        /// </summary>
        /// <returns>A boolean representing the result</returns>
        public bool IsValid()
        {
            if( Signature == null) { return false; }
            if (Owner == null || Metadata == null){ return false; }
            if (Metadata.Name == null || Metadata.Description == null){ return false; }
            if(Metadata.Attributes == null || Metadata.ImageUrl == null){ return false; }
            if (!VerifySignature()) { return false; }
            return true;
        }

        /// <summary>
        /// Returns a JObject equivalent to this <see cref="TokenCreation"/>, similar to <see cref="Transaction"/>
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

        /// <summary>
        /// Calculate the hash for the <see cref="TokenCreation"/>. Only computes <see cref="Owner"/>, <see cref="Timestamp"/>,
        /// <see cref="TokenId"/> and <see cref="Metadata"/>.
        /// The hash, signature are left out of it. Uses SHA3-512
        /// </summary>
        /// <returns>The hash in Hexadecimal</returns>
        public string CalculateHash()
        {
            var sha = new Sha3Digest(512);
            byte[] input2 = Encoding.ASCII.GetBytes($"{this.Owner}-{this.Timestamp}-{this.TokenId}-{this.Metadata}");

            sha.BlockUpdate(input2, 0, input2.Length);
            byte[] result = new byte[64];
            sha.DoFinal(result, 0);

            string hash = BitConverter.ToString(result);
            return hash.Replace("-", "").ToLowerInvariant();
        }

        public void SignTokenCreation(Key privateKey)
        {
            if (privateKey.PubKey.ToHex() != this.Owner)
            {
                throw new Exception("Invalid key");
            }

            var hash = CalculateHash();
            var signature = privateKey.SignMessage(hash);
            Signature = signature;
        }

        /// <summary>
        /// Verifies if the transaction is signed by the owner
        /// </summary>
        /// <param name="pubKey">The public key </param>
        /// <returns>A boolean representing the result</returns>
        public bool VerifySignature()
        {
            PubKey pub = new(this.Owner);
            return pub.VerifyMessage(this.CalculateHash(), this.Signature);
        }
    }
}
