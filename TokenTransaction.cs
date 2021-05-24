using NBitcoin;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodrigoCoin_v2
{
    public class TokenTransaction : BlockChainEvent
    {
        #region Variables
        //in hash
        /// <summary>
        /// The Address that the coins will withdrawed
        /// </summary>
        public string FromAddress { get; set; }


        /// <summary>
        /// The receiver of the coins
        /// </summary>
        public string ToAddress { get; set; }


        /// <summary>
        /// The <see cref="TokenId"/> of the Token being transferred
        /// </summary>
        public Guid TokenId { get; set; }


        /// <summary>
        /// The time when the object <see cref="TokenTransaction"/> was created.
        /// Isn't the time that is added to the block.
        /// </summary>
        public long Timestamp { get; set; }


        //not in hash
        /// <summary>
        /// The hash signed with the <see cref="FromAddress"/>/<see cref="PubKey"/>
        /// </summary>
        public string Signature { get; set; }

        #endregion

        #region Constructors


        /// <summary>
        /// Creates a new Token Transaction. 
        /// </summary>
        /// <param name="fromAddress">Same of the <see cref="PubKey"/></param>
        /// <param name="toAddress">The Address of the receiver</param>
        /// <param name="tokenId">The Id of the token being transferred</param>
        public TokenTransaction(string fromAddress, string toAddress, Guid tokenId)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress;
            TokenId = tokenId;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            EventType = EventType.TokenTransaction;
        }


        /// <summary>
        /// Creates a new transaction. 
        /// </summary>
        /// <param name="fromAddress">Same of the <see cref="PubKey"/></param>
        /// <param name="toAddress">The Address of the receiver</param>
        /// <param name="amount">The Id of the token being transferred</param>
        public TokenTransaction(string fromAddress, PubKey toAddress, Guid tokenId)
        {
            FromAddress = fromAddress;
            ToAddress = toAddress.ToHex();
            TokenId = tokenId;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            EventType = EventType.TokenTransaction;
        }


        /// <summary>
        /// Creates a new transaction. 
        /// </summary>
        /// <param name="fromAddress">Same of the <see cref="PubKey"/></param>
        /// <param name="toAddress">The Address of the receiver</param>
        /// <param name="tokenId">The Id of the token being transferred</param>
        public TokenTransaction(PubKey fromAddress, string toAddress, Guid tokenId)
        {
            FromAddress = fromAddress.ToHex();
            ToAddress = toAddress;
            TokenId = tokenId;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            EventType = EventType.TokenTransaction;
        }


        /// <summary>
        /// Creates a new transaction. 
        /// </summary>
        /// <param name="fromAddress">Same of the <see cref="PubKey"/></param>
        /// <param name="toAddress">The Address of the receiver</param>
        /// <param name="tokenId">The Id of the token being transferred</param>
        public TokenTransaction(PubKey fromAddress, PubKey toAddress, Guid tokenId)
        {
            FromAddress = fromAddress.ToHex();
            ToAddress = toAddress.ToHex();
            TokenId = tokenId;
            EventType = EventType.TokenTransaction;
            Timestamp = DateTime.UtcNow.ToFileTimeUtc();
        }


        #endregion


        /// <summary>
        /// Sign the current transaction
        /// </summary>
        /// <param name="privateKey">The private key used to sign the transaction</param>
        public void SignTransaction(Key privateKey)
        {
            //check is the owner making the transaction
            if (privateKey.PubKey.ToHex() != this.FromAddress)
            {
                throw new Exception("Invalid key");
            }

            var HashTransaction = CalculateHash();
            var signature = privateKey.SignMessage(HashTransaction);
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
            if (this.FromAddress == null || this.ToAddress == null || TokenId == new Guid()) { return false; }
            //check signature
            if (!VerifySignature()) { return false; }

            return true;
        }


        /// <summary>
        /// Calculate the hash for the transaction. Only computes, from, to addresses and the amount.
        /// The hash, signature are left out of it. Uses SHA3-512
        /// </summary>
        /// <returns>The hash in Hexadecimal</returns>
        public string CalculateHash()
        {
            var sha = new Sha3Digest(512);
            byte[] input2 = Encoding.ASCII.GetBytes($"{FromAddress}-{ToAddress}-{TokenId}-{Timestamp}");

            sha.BlockUpdate(input2, 0, input2.Length);
            byte[] result = new byte[64];
            sha.DoFinal(result, 0);

            string hash = BitConverter.ToString(result);
            return hash.Replace("-", "").ToLowerInvariant();
        }


        /// <summary>
        /// Verifies if the transaction is signed by the owner
        /// </summary>
        /// <param name="pubKey">The public key </param>
        /// <returns>A boolean representing the result</returns>
        public bool VerifySignature()
        {
            PubKey pubKey = new(this.FromAddress);
            return pubKey.VerifyMessage(CalculateHash(), this.Signature);
        }
    }
}
