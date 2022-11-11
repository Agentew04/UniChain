using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unichain.Core;

namespace Unichain.Events {
    public class CurrencyTransaction : ITransaction {

        #region default properties
        
        public User Actor { get; set; }
        public double Fee { get; set; }
        public long Timestamp { get; set; } = DateTime.UtcNow.Ticks;
        public string TypeId { get; set; } = "transaction.currency";
        public string? Signature { get; set; }

        #endregion

        #region custom properties

        /// <summary>
        /// The address that will receive the funds
        /// </summary>
        public string ToAddress { get; set; }

        /// <summary>
        /// The quantity of currency that will be transferred, need to be bigger than the user's balance
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// A optional message to be included in the transaction
        /// </summary>
        public string Message { get; set; }

        #endregion

        #region constructor

        public CurrencyTransaction(User actor,
            double fee,
            string receiverAddress,
            double amount, 
            string message = ""){
            Actor = actor;
            Fee = fee;
            ToAddress = receiverAddress;
            Amount = amount;
            Message = message;
        }

        #endregion

        #region Methods

        public string CalculateHash() {
            var bytes = Encoding.UTF8.GetBytes($"{Actor.Address}-{Message}-{Amount}-{Timestamp}");
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        public bool IsValid(Blockchain blockchain) {
            if (Amount <= 0)
                return false;

            double balance = blockchain.GetBalance(Actor.Address);
            if(balance < Amount)
                return false;
            
            if(balance < Fee + Amount) 
                return false;
            
            if (Signature is null)
                return false;
            
            string hash = CalculateHash();
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
