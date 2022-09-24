using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using Unichain.Core;

namespace Unichain.Events
{
    public class NFTBurn : ITransaction
    {
        #region default properties

        public User Actor { get; set; }
        public double Fee { get; set; }
        public long Timestamp { get; set; } = DateTime.UtcNow.Ticks;
        public string TypeId { get; set; } = "transaction.nft.burn";
        public string? Signature { get; set; }

        #endregion

        #region custom properties

        /// <summary>
        /// The unique id of the NFT that is going to be burned.
        /// </summary>
        public Guid NFTId { get; set; }

        #endregion

        #region constructor

        public NFTBurn(User actor,
            double fee,
            Guid nftId) {
            Actor = actor;
            Fee = fee;
            NFTId = nftId;
        }

        #endregion

        #region methods

        public string CalculateHash() {
            var bytes = Encoding.UTF8.GetBytes($"{Actor.Address}-{NFTId}-{Timestamp}");
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        public bool IsValid(Blockchain blockchain) {
            bool exists = blockchain.IsNFTMinted(NFTId);
            bool isOwner = blockchain.IsNFTOwner(NFTId, Actor.Address);
            bool isBurned = blockchain.IsNFTBurned(NFTId);
            double balance = blockchain.GetBalance(Actor.Address);
            if (!exists || !isOwner || isBurned)
                return false;
            if (balance < Fee)
                return false;
            
            if (Signature is null)
                return false;
            var hash = CalculateHash();
            return Actor.VerifySignature(hash, Signature);
        }

        public void SignTransaction(PrivateKey? key = null) {
            string hash = CalculateHash();
            if (key is null)
                Signature = Actor.SignString(hash);
            else
                Signature = key.Sign(hash);
        }

        #endregion
    }
}