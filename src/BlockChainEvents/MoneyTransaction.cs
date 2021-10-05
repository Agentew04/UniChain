using System;
using System.Text;
using NBitcoin;
using Org.BouncyCastle.Crypto.Digests;
using RodrigoChain.Exceptions;

namespace RodrigoChain
{
    public class MoneyTransaction : BaseBlockChainEvent
    {
        #region Variables
        //in hash
        /// <summary>
        /// The Address that the coins will withdrawed
        /// </summary>
        public Address FromAddress { get; set; }


        /// <summary>
        /// The receiver of the coins
        /// </summary>
        public Address ToAddress { get; set; }


        /// <summary>
        /// The amount of coins being transferred
        /// </summary>
        public int Amount { get; set; }


        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of a Transaction
        /// </summary>
        /// <param name="user">The user causing the action</param>
        /// <param name="to">The target addres to receive the money</param>
        /// <param name="amount">The amount of money to be transferred</param>
        /// <returns></returns>
        public MoneyTransaction(User user, Address to, int amount) : base(EventType.MoneyTransaction,user)
        {
            EventType=EventType.MoneyTransaction;
            ActionOwner=user;
            Amount=amount;
            ToAddress=to;
            FromAddress=user.Address;
        }

        #endregion

        #region Methods
        
        public override void SignEvent(User user)
        {
            if ( user != this.FromAddress)
            {
                throw new InvalidKeyException();
            }

            var hashTransaction = CalculateHash();
            var signature = user.SignMessage(hashTransaction);
            Signature = signature;
        }

        public override bool IsValid()
        {
            //check addresses and amount
            if(this.FromAddress.PublicKey=="network" && this.ToAddress.PublicKey != null) { return true; }
            if (Signature == null) { return false; }
            if (this.FromAddress.IsNull() || this.ToAddress.IsNull() || Amount <= 0) { return false; }
            //check signature
            if (!VerifySignature()) { return false; }

            return true;
        }

        public override string CalculateHash()
        {
            var sha = new Sha3Digest(512);
            byte[] input2 = Encoding.ASCII.GetBytes($"{FromAddress}-{ToAddress}-{Amount}-{Timestamp}");

            sha.BlockUpdate(input2, 0, input2.Length);
            byte[] result = new byte[64];
            sha.DoFinal(result, 0);

            string hash = BitConverter.ToString(result);
            return hash.Replace("-", "").ToLowerInvariant();
        }
        #endregion
    }
}
