using System;
using System.Security.Cryptography;
using System.Text;
using Unichain.Core;

namespace Unichain.Events
{
    public class NFTBurn : BaseBlockChainEvent
    {
        #region Vars

        /// <summary>
        /// The unique id for this NFT
        /// </summary>
        /// <value></value>
        public Guid NFTId { get; set; }

        /// <summary>
        /// The Address that burned this NFT
        /// </summary>
        public Address BurnerAddress { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of a NFT Burn
        /// </summary>
        /// <param name="user">The user executing the action</param>
        /// <param name="nftId">The unique id of the NFT to be burned</param>
        public NFTBurn(User user, Guid nftId) : base(EventType.NFTBurn, user)
        {
            ActionOwner = user;
            Timestamp = DateTime.UtcNow.Ticks;
            NFTId = nftId;
            BurnerAddress = user.Address;
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
        public override bool IsValid(Blockchain blockchain)
        {
            //check addresses and amount

            if (!blockchain.IsNFTMinted(NFTId)) return false;
            if (blockchain.IsNFTBurned(NFTId)) return false;
            if (!blockchain.IsNFTOwner(NFTId, BurnerAddress)) return false;
            if (Signature == null) return false;
            if (NFTId == Guid.Empty) return false;
            if (NFTId == new Guid()) return false;
            if (!VerifySignature()) return false;

            return true;
        }

        public override string CalculateHash()
        {
            //calculate sha512 hash using nftid, timestamp and burneraddress
            var bytes = System.Text.Encoding.UTF8.GetBytes($"{NFTId}-{Timestamp}");
            using var hash = SHA512.Create();
            var hashedInputBytes = hash.ComputeHash(bytes);

            // Convert to text
            // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
            var hashedInputStringBuilder = new StringBuilder(128);
            foreach (var b in hashedInputBytes)
                hashedInputStringBuilder.Append(b.ToString("X2"));
            return hashedInputStringBuilder.ToString();
        }

        #endregion
    }
}