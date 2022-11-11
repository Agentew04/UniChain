using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Unichain.Core;
using Unichain.Exceptions;

namespace Unichain.Events
{
    public class PoolCreate : ITransaction
    {
        #region default variables

        public User Actor { get; set; }
        public double Fee { get; set; }
        public long Timestamp { get; set; } = DateTime.UtcNow.Ticks;
        public string TypeId { get; set; } = "transaction.pool.create";
        public string? Signature { get; set; }

        #endregion

        #region custom variables

        /// <summary>
        /// The unique Id for this Pool
        /// </summary>
        public Guid PoolId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The name of the pool.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// An optional description for the pool.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// An <see cref="IEnumerable{T}"/> containing all the valid options that can be voted.
        /// </summary>
        public IEnumerable<string> Options { get; set; }

        /// <summary>
        /// The minimum balance an account must have to be able to vote for this pool
        /// </summary>
        public double MinimumBalance { get; set; } = 0;

        /// <summary>
        /// A <see cref="DateTime.Ticks"/> specifying the end of the pool. All votes cast 
        /// after this time will be considered invalid.
        /// </summary>
        public long? Deadline { get; set; }

        #endregion

        #region constructor

        public PoolCreate(User actor,
            double fee,
            string name,
            IEnumerable<string> options,
            string description = "",
            double minimumBalance = 0,
            long? deadline = null) {
            Actor = actor;
            Fee = fee;
            Name = name;
            Options = options;
            Description = description;
            MinimumBalance = minimumBalance;
            Deadline = deadline;
        }

        #endregion

        #region methods

        public string CalculateHash() {
            var bytes = Encoding.UTF8.GetBytes($"{Actor.Address}-{PoolId}-{Name}-{JsonSerializer.Serialize(Options)}" +
                $"-{Description}-{MinimumBalance}-{Deadline}-{Timestamp}");
            using var sha512 = SHA256.Create();
            byte[] hash = sha512.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        public bool IsValid(Blockchain blockchain) {
            var balance = blockchain.GetBalance(Actor.Address);
            
            // todo insuficient balance exception?
            if (balance < Fee)
                return false;
            if (Options.Any() || PoolId == Guid.Empty || string.IsNullOrWhiteSpace(Name))
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