using RodrigoChain;

namespace RodrigoChain
{
    public abstract class BaseBlockChainEvent
    {
        public EventType EventType { get; set;}

        public User ActionOwner { get; set;}

        /// <summary>
        /// The time when the object <see cref="MoneyTransaction"/> was created.
        /// Isn't the time that is added to the block.
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// The hash signed with the Address/Public Key
        /// </summary>
        public string Signature { get; set; } /*is not included in the hash*/

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
        public abstract bool IsValid();

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

        public EventType ToEventType(){
            System.Type type = this.GetType();
            if(type==typeof(MoneyTransaction)){
                return EventType.MoneyTransaction;
            }else if(type == typeof(NFTMinting)){
                return EventType.NFTMinting;
            }else if(type==typeof(NFTTransaction)){
                return EventType.NFTTransaction;
            }else if(type==typeof(VoteCreation)){
                return EventType.VoteCreated;
            }else if(type==typeof(VoteCast)){
                return EventType.VoteCast;
            }else{
                return EventType.MoneyTransaction;
            }
        }
    }
}
