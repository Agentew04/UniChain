using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace RodrigoCoin_v2
{
    public class Block
    {
        public int Index { get; set; }
        public long TimeStamp { get; set; }
        public string PreviousHash { get; set; }
        public string Hash { get; set; }
        public int Nonce { get; set; } = 0;
        public IList<BlockChainEvent> Transactions { get; set; }

        public Block(string previousHash, IList<BlockChainEvent> transactions)
        {
            Index = 0;
            TimeStamp = DateTime.UtcNow.ToFileTimeUtc();
            PreviousHash = previousHash;
            Transactions = transactions;
            Hash = CalculateHash();
        }


        /// <summary>
        /// Check if all the <see cref="Transaction"/>, <seealso cref="TokenCreation"/> are valid
        /// </summary>
        /// <returns>A booleand representing the result</returns>
        public bool HasValidTransactions()
        {
            foreach(var x in Transactions)
            {
                if(x.GetType() == typeof(Transaction))
                {
                    if (!(((Transaction)x).IsValid()))
                    {
                        return false;
                    }else { continue; }
                }
                if(x.GetType() == typeof(TokenCreation))
                {
                    if (!(((TokenCreation)x).IsValid()))
                    {
                        return false;
                    }else { continue; }
                }
                //TODO ADD TOKEN TRANSACTION
            }
            return true;
        }


        /// <summary>
        /// Calculates the Hash of the transaction
        /// </summary>
        /// <returns></returns>
        public string CalculateHash()
        {
            var sha = new Sha3Digest(512);
            byte[] input2 = Encoding.ASCII.GetBytes($"{TimeStamp}-{PreviousHash ?? ""}-{JsonConvert.SerializeObject(Transactions)}-{Nonce}");

            sha.BlockUpdate(input2, 0, input2.Length);
            byte[] result = new byte[64];
            sha.DoFinal(result, 0);

            string hash = BitConverter.ToString(result);
            return hash.Replace("-", "").ToLowerInvariant();
        }


        /// <summary>
        /// Mine the current block
        /// </summary>
        /// <param name="difficulty">The amount of 0s in the start of the hash(AKA difficulty)</param>
        public void MineBlock(int difficulty)
        {
            var leadingZeros = new string('0', difficulty);
            while (this.Hash == null || this.Hash.Substring(0, difficulty) != leadingZeros)
            {
                this.Nonce++;
                this.Hash = this.CalculateHash();
            }
        }
    }
}
