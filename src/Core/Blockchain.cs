using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Unichain.Core;
using Unichain.Events;
using Unichain.Exceptions;

namespace Unichain.Core;

/// <summary>
/// The main class of the Unichain library. It represents an instance of a blockchain.
/// Also contains the methods for searching and traversing the chain.
/// </summary>
public partial class Blockchain
{
    /// <summary>
    /// Represents the Chain of <see cref="Block"/> for this blockchain
    /// </summary>
    public List<Block> Chain { get; init; } = new List<Block>();
    /// <summary>
    /// The difficulty, or the number of zeros in the start of the hash
    /// </summary>
    public int Difficulty { set; get; } = 2;
    /// <summary>
    /// The amount of coins rewarded to the miner when a block is created
    /// </summary>
    public double Reward { get; set; } = 100;
    /// <summary>
    /// A list with all pending <see cref="ITransaction"/>, <see cref="TokenCreation"/> and
    /// <see cref="TokenTransaction"/>.
    /// </summary>
    private IList<ITransaction> PendingTransactions = new List<ITransaction>();

    public Blockchain(int difficulty, double reward)
    {
        Block origin = new("", new List<ITransaction>(), "");
        origin.MineBlock(difficulty);
        Chain.Add(origin);
        Difficulty = difficulty;
        Reward = reward;
    }

    [JsonConstructor]
    public Blockchain() { }

    public void PrintChain()
    {
        Console.WriteLine(JsonConvert.SerializeObject(this.Chain));
    }

    /// <summary>
    /// Returns the latest block in the chain
    /// </summary>
    /// <returns>The latest block</returns>
    private Block GetLatestBlock()
    {
        return Chain[^1];
    }

    #region add to chain

    /// <summary>
    /// Adds a new event to the pending transactions list
    /// </summary>
    /// <exception cref="InvalidTransactionException">Thrown when the transaction is invalid</exception>
    public void AddEvent(ITransaction e)
    {
        //add to the pending transactions
        if (e.IsValid(this))
        {
            PendingTransactions.Add(e);
        }
        else throw new InvalidTransactionException("Invalid transaction!");
    }
    public void AddEvent(IEnumerable<ITransaction> events) { 
        foreach(var e in events) 
            AddEvent(e);
    }

    #endregion

    /// <summary>
    /// Checks all the transactions in <see cref="PendingTransactions"/> and creates a new block
    /// in the blockchain
    /// </summary>
    /// <param name="minerAddress">The address that will receive all the rewards</param>
    /// <exception cref="ArgumentNullException">Throuw when the miner Address is null</exception>
    /// <exception cref="InvalidTransactionException">Thrown when the block made is not valid</exception>
    public void MinePendingTransactions(string minerAddress)
    {
        if (!PublicKey.IsAddressValid(minerAddress))
            throw new ArgumentException("Invalid miner address!");
        
        Block block = new(GetLatestBlock().Hash, PendingTransactions, minerAddress);
        if (!block.HasValidTransactions(this))
            throw new InvalidTransactionException("One of the transactions is not valid!");

        Block latestBlock = GetLatestBlock();
        block.Index = latestBlock.Index + 1;
        block.PreviousHash = latestBlock.Hash;
        block.MineBlock(this.Difficulty);
        Chain.Add(block);

        PendingTransactions = new List<ITransaction>();
    }

    /// <summary>
    /// Checks if the blockchain is valid, if the block hashes match
    /// </summary>
    /// <returns>Returns <see cref="true"/> if everything is valid or
    /// <see cref="false"/> if a hash doesn't match</returns>
    public bool IsValid()
    {
        for (int i = 1; i < Chain.Count; i++)
        {
            Block currentBlock = Chain[i];
            Block previousBlock = Chain[i - 1];

            if (currentBlock.Hash != currentBlock.CalculateHash())
            {
                return false;
            }

            if (currentBlock.PreviousHash != previousBlock.Hash)
            {
                return false;
            }
        }
        return true;
    }

    public override string ToString() {
        return JsonConvert.SerializeObject(this);
    }
}
