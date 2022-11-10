using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Unichain.Events;

namespace Unichain.Core;

public class Block
{
    public int Index { get; set; }
    public long Timestamp { get; set; } = DateTime.UtcNow.Ticks;
    public string PreviousHash { get; set; } = "";
    public string Hash { get; set; } = "";
    public int Nonce { get; set; } = 0;
    public IList<ITransaction> Events { get; set; } = new List<ITransaction>();
    public string Miner { get; set; } = "";
    public double CollectedFees { get; set; } = 0.0;

    public Block(string previousHash, IList<ITransaction> events, string minerAddress)
    {
        Index = 0;
        PreviousHash = previousHash;
        Events = events;
        Miner = minerAddress;
    }

    public Block() { }

    /// <summary>
    /// Check if all the <see cref="Transaction"/>, <seealso cref="TokenCreation"/> are valid
    /// </summary>
    /// <returns>A booleand representing the result</returns>
    public bool HasValidTransactions(Blockchain blockchain)
    {
        if (string.IsNullOrWhiteSpace(Miner))
            return false;
        bool isValid = Events.All(x => x.IsValid(blockchain));
        return isValid;
    }


    /// <summary>
    /// Calculates the Hash of the transaction
    /// </summary>
    /// <returns></returns>
    public string CalculateHash()
    {
        //calculate sha512 hash using timestamp, previous hash, nonce and events(json)
        var bytes = Encoding.UTF8.GetBytes($"{Timestamp}-{PreviousHash ?? ""}-{JsonConvert.SerializeObject(Events)}-{Nonce}-{Miner}-{CollectedFees}");
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

        bool checkHash()
        {
            string binarystring = string.Join("",
              Hash.Select(
                c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')
              )
            );

            return binarystring[..difficulty] == leadingZeros;
        }

        do {
            Nonce++;
            Hash = CalculateHash();
        } while (!checkHash());
    }

    public override string ToString()
    {
        //convert to json
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }
}
