using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Unichain.Core;
using Unichain.Events;

namespace Unichain
{
    public class Block
    {
        public int Index { get; set; }
        public long Timestamp { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public int Nonce { get; set; } = 0;
        public IList<ITransaction> Events { get; set; }

        public Block(string previousHash, IList<ITransaction> events)
        {
            Index = 0;
            Timestamp = DateTime.UtcNow.Ticks;
            PreviousHash = previousHash;
            Events = events;
            Hash = CalculateHash();
        }

        /// <summary>
        /// Creates an empty instance of the <see cref="Block"/> class
        /// </summary>
        public Block()
        {

        }


        /// <summary>
        /// Check if all the <see cref="Transaction"/>, <seealso cref="TokenCreation"/> are valid
        /// </summary>
        /// <returns>A booleand representing the result</returns>
        public bool HasValidTransactions(Blockchain blockchain)
        {
            bool isValid = Events.Any(x => !x.IsValid(blockchain));
            return isValid;
        }


        /// <summary>
        /// Calculates the Hash of the transaction
        /// </summary>
        /// <returns></returns>
        public string CalculateHash()
        {
            //calculate sha512 hash using timestamp, previous hash, nonce and events(json)
            var bytes = Encoding.UTF8.GetBytes($"{Timestamp}-{PreviousHash ?? ""}-{JsonConvert.SerializeObject(Events)}-{Nonce}");
            using var hash = SHA512.Create();
            var hashedInputBytes = hash.ComputeHash(bytes);

            return Convert.ToHexString(hashedInputBytes);
        }


        /// <summary>
        /// Mine the current block
        /// </summary>
        /// <param name="difficulty">The amount of 0s in the start of the hash(AKA difficulty)</param>
        public void MineBlock(int difficulty)
        {
            var leadingZeros = new string('0', difficulty);
            
            bool checkHash() {
                string binarystring = string.Join("",
                  Hash.Select(
                    c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')
                  )
                );

                return binarystring[..difficulty] == leadingZeros;
            }
            
            while (checkHash())
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
