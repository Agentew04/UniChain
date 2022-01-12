using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Unichain.Core;

namespace Unichain
{
    public class Block
    {
        public int Index { get; set; }
        public long Timestamp { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public int Nonce { get; set; } = 0;
        public IList<BaseBlockChainEvent> Events { get; set; }

        public Block(string previousHash, IList<BaseBlockChainEvent> events)
        {
            Index = 0;
            Timestamp = DateTime.UtcNow.Ticks;
            PreviousHash = previousHash;
            Events = events;
            Hash = CalculateHash();
        }


        /// <summary>
        /// Check if all the <see cref="Transaction"/>, <seealso cref="TokenCreation"/> are valid
        /// </summary>
        /// <returns>A booleand representing the result</returns>
        public bool HasValidTransactions(Blockchain blockchain)
        {
            foreach (var x in Events)
            {
                if (x.IsNetwork)
                {
                    continue;
                }
                if (!x.IsValid(blockchain))
                {
                    return false;
                }
            }
            return true;
        }


        /// <summary>
        /// Calculates the Hash of the transaction
        /// </summary>
        /// <returns></returns>
        public string CalculateHash()
        {
            //calculate sha512 hash using nftid, timestamp and burneraddress
            var bytes = System.Text.Encoding.UTF8.GetBytes($"{Timestamp}-{PreviousHash ?? ""}-{JsonConvert.SerializeObject(Events)}-{Nonce}");
            using var hash = SHA512.Create();
            var hashedInputBytes = hash.ComputeHash(bytes);

            // Convert to text
            // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
            var hashedInputStringBuilder = new StringBuilder(128);
            foreach (var b in hashedInputBytes)
                hashedInputStringBuilder.Append(b.ToString("X2"));
            return hashedInputStringBuilder.ToString();
        }


        /// <summary>
        /// Mine the current block
        /// </summary>
        /// <param name="difficulty">The amount of 0s in the start of the hash(AKA difficulty)</param>
        public void MineBlock(int difficulty)
        {
            var leadingZeros = new string('0', difficulty);
            while (string.IsNullOrWhiteSpace(Hash) || Hash[..difficulty] != leadingZeros)
            {
                Nonce++;
                Hash = CalculateHash();
            }
        }

        public override string ToString()
        {
            //convert to json
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
