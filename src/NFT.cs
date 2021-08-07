using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodrigoCoin_v2
{
    public struct NFT:BlockChainEvent
    {
        /// <summary>
        /// A GUID/UUID for this token, randomly generated
        /// </summary>
        public Guid TokenId { get; set; }

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
        /// The metadata for this token, defines how it looks like
        /// </summary>
        public TokenMetadata Metadata { get; set; }
        public EventType EventType { get;private set;}

        public NFT(string owner, TokenMetadata metadata)
        {
            this.TokenId = Guid.NewGuid();
            this.Owner = owner;
            this.Metadata = metadata;
            this.Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            EventType=EventType.NFT;
        }

        public static explicit operator NFT(TokenCreation v)
        {
            return new NFT()
            {
                Metadata = v.Metadata,
                Owner = v.Owner,
                Timestamp = v.Timestamp,
                TokenId = v.TokenId
            };
        }

        public static explicit operator NFT(TokenTransaction v)
        {
            return new NFT()
            {
                Owner = v.ToAddress,
                TokenId = v.TokenId,
                Timestamp = v.Timestamp
            };
        }
    }
}
