using Ardalis.SmartEnum.JsonNet;
using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using Unichain.Events;
using Unichain.Exceptions;

namespace Unichain.Core
{
    [JsonConverter(typeof(JsonSubtypes), "EventType")]
    [JsonSubtypes.KnownSubType(typeof(Transaction), nameof(EventType.Transaction))]
    [JsonSubtypes.KnownSubType(typeof(NFTMint), nameof(EventType.NFTMint))]
    [JsonSubtypes.KnownSubType(typeof(NFTTransfer), nameof(EventType.NFTTransfer))]
    [JsonSubtypes.KnownSubType(typeof(NFTBurn), nameof(EventType.NFTBurn))]
    [JsonSubtypes.KnownSubType(typeof(PoolOpen), nameof(EventType.PoolOpen))]
    [JsonSubtypes.KnownSubType(typeof(PoolVote), nameof(EventType.PoolVote))]
    public class BaseBlockChainEvent
    {
        [JsonConverter(typeof(SmartEnumNameConverter<EventType, int>))]
        public EventType EventType { get; set; }

        public User ActionOwner { get; set; }

        /// <summary>
        /// The time when the object <see cref="Transaction"/> was created.
        /// Isn't the time that is added to the block.
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// The hash signed with the Address/Public Key, it isn't included in the hash
        /// </summary>
        public string Signature { get; set; } /*is not included in the hash*/

        /// <summary>
        /// Determines if this event is originated from the blockchain.
        /// </summary>
        /// <value></value>
        public bool IsNetwork { get; set; }

        public BaseBlockChainEvent(EventType eventType, User user)
        {
            EventType = eventType;
            ActionOwner = user;
        }

        /// <summary>
        /// Calculate the hash for the transaction.
        /// The hash, signature are left out of it. Uses SHA3-512
        /// </summary>
        /// <returns>The hash in Hexadecimal</returns>
        public virtual string CalculateHash() { throw new NotImplementedException(); }

        /// <summary>
        /// Checks if the current transaction is valid
        /// </summary>
        /// <returns>A boolean representing the result</returns>
        public virtual bool IsValid(Blockchain blockchain) { throw new NotImplementedException(); }

        /// <summary>
        /// Sign the current event
        /// </summary>
        /// <param name="privateKey">The private key used to sign the transaction</param>
        /// <exception cref="InvalidKeyException"></exception>
        public virtual void SignEvent(User user) { throw new NotImplementedException(); }

        /// <summary>
        /// Verifies if the transaction is signed by the owner
        /// </summary>
        /// <param name="pubKey">The public key </param>
        /// <returns>A boolean representing the result</returns>
        public bool VerifySignature()
        {
            return ActionOwner.Address.VerifySign(CalculateHash(), Signature);
        }

        /// <summary>
        /// Returns the current object in its JSON serialized form
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
