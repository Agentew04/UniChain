using System;
using System.Security.Cryptography;
using System.Text;
using Unichain.Core;

namespace Unichain.Events
{
    public class NFTTransfer : ITransaction
    {
        #region default properties

        public User Actor { get; set; }
        public double Fee { get; set; }
        public long Timestamp { get; set; } = DateTime.UtcNow.Ticks;
        public string TypeId { get; set; } = "transaction.nft.transfer";
        public string? Signature { get; set; }

        #endregion

        #region custom properties

        /// <summary>
        /// The unique Id of the NFT that is going to be transfered.
        /// </summary>
        public Guid NFTId { get; set; }

        /// <summary>
        /// The address that will be the new owner of the NFT.
        /// </summary>
        public string ToAddress { get; set; }

        #endregion

        #region constructor

        public NFTTransfer(User actor,
            double fee,
            string toAddress,
            Guid nftId) {
            Actor = actor;
            Fee = fee;
            ToAddress = toAddress;
            NFTId = nftId;
        }

        #endregion

        #region methods

        public string CalculateHash() {
            var bytes = Encoding.UTF8.GetBytes($"{Actor.Address}-{NFTId}-{Timestamp}-{ToAddress}");
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
            if(balance < Fee)
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
