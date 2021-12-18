using RodrigoChain.Events;

namespace RodrigoChain.Core
{
    public abstract class BaseBlockChainEvent
    {
        public EventType EventType { get; set;}

        public User ActionOwner { get; set;}

        /// <summary>
        /// The time when the object <see cref="Transaction"/> was created.
        /// Isn't the time that is added to the block.
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// The hash signed with the Address/Public Key
        /// </summary>
        public string Signature { get; set; } /*is not included in the hash*/

        /// <summary>
        /// Determines if this event is originated from the blockchain.
        /// </summary>
        /// <value></value>
        public bool IsNetwork { get; set; }

        public BaseBlockChainEvent(EventType eventType, User user){
            EventType=eventType;
            ActionOwner=user;
        }

        /// <summary>
        /// Calculate the hash for the transaction.
        /// The hash, signature are left out of it. Uses SHA3-512
        /// </summary>
        /// <returns>The hash in Hexadecimal</returns>
        public abstract string CalculateHash();

        /// <summary>
        /// Checks if the current transaction is valid
        /// </summary>
        /// <returns>A boolean representing the result</returns>
        public abstract bool IsValid(Blockchain blockchain);

        /// <summary>
        /// Sign the current event
        /// </summary>
        /// <param name="privateKey">The private key used to sign the transaction</param>
        public abstract void SignEvent(User user);

        /// <summary>
        /// Verifies if the transaction is signed by the owner
        /// </summary>
        /// <param name="pubKey">The public key </param>
        /// <returns>A boolean representing the result</returns>
        public bool VerifySignature()
        {
            return this.ActionOwner.Address.VerifySign(CalculateHash(), this.Signature);
        }
    }
}
