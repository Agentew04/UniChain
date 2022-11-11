using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Unichain.Core;
using Unichain.Exceptions;

namespace Unichain.Events
{
    public class PoolVote : ITransaction
    {
        #region default variables

        public User Actor { get; set; }
        public double Fee { get; set; }
        public long Timestamp { get; set; } = DateTime.UtcNow.Ticks;
        public string TypeId { get; set; } = "transaction.pool.vote";
        public string? Signature { get; set; }

        #endregion

        #region custom variables

        public Guid PoolId { get; set; }

        public int OptionSelected { get; set; }

        #endregion

        #region constructor

        public PoolVote(User actor,
            double fee,
            Guid poolId,
            int optionSelected) {
            Actor = actor;
            Fee = fee;
            PoolId = poolId;
            OptionSelected = optionSelected;
        }

        #endregion

        #region methods

        public string CalculateHash() {
            var bytes = Encoding.UTF8.GetBytes($"{Actor.Address}-{PoolId}-{OptionSelected}-{Timestamp}");
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool IsValid(Blockchain blockchain) {
            double balance = blockchain.GetBalance(Actor.Address);
            PoolCreate? pCreate = blockchain.Find<PoolCreate>(x => x.PoolId == PoolId).FirstOrDefault();
            if (balance < Fee)
                return false;
            if (pCreate is null)
                return false;
            if (OptionSelected < 0 || OptionSelected >= pCreate?.Options.Count())
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