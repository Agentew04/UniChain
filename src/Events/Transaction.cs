using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;
using Unichain.Core;
using Unichain.Exceptions;

namespace Unichain.Events
{
    public class Transaction : BaseBlockChainEvent
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
        /// The amount of coins being transferred
        /// </summary>
        public double Amount { get; set; }


        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of a Transaction
        /// </summary>
        /// <param name="user">The user causing the action</param>
        /// <param name="to">The target addres to receive the money</param>
        /// <param name="amount">The amount of money to be transferred</param>
        /// <returns></returns>
        public Transaction(User user, string to, double amount) : base(EventType.Transaction, user)
        {
            EventType = EventType.Transaction;
            ActionOwner = user;
            Amount = amount;
            ToAddress = to;
            FromAddress = user.Address;
            Timestamp = DateTime.UtcNow.Ticks;
        }

        [JsonConstructor]
        public Transaction(string FromAddress, string ToAddress, double Amount,
            EventType EventType, long Timestamp, string Signature, bool IsNetwork) : base(EventType.Transaction, null)
        {
            this.FromAddress = FromAddress;
            this.ToAddress = ToAddress;
            this.Amount = Amount;
            this.EventType = EventType;
            this.Timestamp = Timestamp;
            this.Signature = Signature;
            this.IsNetwork = IsNetwork;
        }

        #endregion

        #region Methods

        public override void SignEvent(User user)
        {
            if (user.Address != this.FromAddress)
            {
                throw new InvalidKeyException();
            }

            var hashTransaction = CalculateHash();
            var signature = user.SignMessage(hashTransaction);
            Signature = signature;
        }

        public override bool IsValid(Blockchain blockchain)
        {
            if (string.IsNullOrWhiteSpace(FromAddress) || string.IsNullOrWhiteSpace(ToAddress) || Amount <= 0)
            {
                return false;
            }
            if (Signature == null || !VerifySignature())
            {
                return false;
            }

            // check if the address has enough balance
            if (!blockchain.HasEnoughBalance(FromAddress, Amount))
            {
                return false;
            }
            return true;
        }

        public override string CalculateHash()
        {
            //calculate sha512 hash using nftid, timestamp and burneraddress
            var bytes = System.Text.Encoding.UTF8.GetBytes($"{FromAddress}-{ToAddress}-{Amount}-{Timestamp}");
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
