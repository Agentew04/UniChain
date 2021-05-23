using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace RodrigoCoin_v2
{
    public class Blockchain
    {
        /// <summary>
        /// Represents the Chain of <see cref="Block"/> for this blockchain
        /// </summary>
        public IList<Block> Chain { get; set; }
        /// <summary>
        /// The difficulty, or the number of zeros in the start of the hash
        /// </summary>
        public int Difficulty { set; get; } = 2;
        /// <summary>
        /// The amount of coins rewarded to the miner when a block is created
        /// </summary>
        public int Reward { get; set; } = 100;
        /// <summary>
        /// A list with all pending <see cref="Transaction"/>, <see cref="TokenCreation"/> and
        /// <see cref="TokenTransaction"/>.
        /// </summary>
        private IList<object> PendingTransactions = new List<object>();



        public Blockchain()
        {
            Chain = new List<Block>
            {
                new Block(null, null)
            };
        }

        /// <summary>
        /// Returns the latest block in the chain
        /// </summary>
        /// <returns>The latest block</returns>
        private Block GetLatestBlock()
        {
            return Chain[Chain.Count - 1];
        }


        /// <summary>
        /// Add a new transaction on the <see cref="PendingTransactions"/> to be processed later
        /// </summary>
        /// <param name="transaction">The transaction to be added</param>
        public void AddTransaction(Transaction transaction)
        {
            if(transaction.FromAddress == "network") { throw new Exception("You cannot act as the network"); }
            if (!transaction.IsValid()) { throw new Exception("The transaction is not valid"); }
            PendingTransactions.Add(transaction);
        }

        /// <summary>
        /// Add a new Token(NFT) on the <see cref="PendingTransactions"/> to be processed later
        /// </summary>
        /// <param name="tokenCreation">The token to be added, type <see cref="TokenCreation"/></param>
        public void AddToken(TokenCreation tokenCreation)
        {
            if(!tokenCreation.IsValid()){ throw new Exception("The token creation is not valid"); }
            PendingTransactions.Add(tokenCreation);
        }

        /// <summary>
        /// Checks all the transactions in <see cref="PendingTransactions"/> and creates a new block
        /// in the blockchain
        /// </summary>
        /// <param name="minerAddress">The address that will receive all the rewards</param>
        public void MinePendingTransactions(string minerAddress)
        {
            if (minerAddress == null) { throw new Exception("Miner Address cannot be null"); }
            PendingTransactions.Insert(0, new Transaction("network", minerAddress, this.Reward));
            Block block = new(GetLatestBlock().Hash, PendingTransactions);
            if (!block.HasValidTransactions()) { throw new Exception("A transaction is not valid"); }

            Block latestBlock = GetLatestBlock();
            block.Index = latestBlock.Index + 1;
            block.PreviousHash = latestBlock.Hash;
            block.MineBlock(this.Difficulty);
            Chain.Add(block);

            PendingTransactions = new List<object>();
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


        /// <summary>
        /// Iterates in the blockchain and sees which NFTs the address has.
        /// Purely informational, like <see cref="GetBalance(string)"/>
        /// </summary>
        /// <param name="address">The address to be looked up</param>
        /// <returns>A <see cref="IList{NFT}"/> containing all the nfts the user has</returns>
        public IList<NFT> GetNFTs(string address)
        {
            IList<NFT> nftsowned = new List<NFT>();

            //get all nfts
            foreach (Block block in this.Chain)
            {
                if (block.Transactions is null)
                {
                    continue;
                }
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.GetType() == typeof(TokenCreation))
                    {
                        NFT nft = (NFT)transaction;
                        if(nft.Owner == address)
                        {
                            nftsowned.Add(nft);
                        }
                    }
                    else { continue; }
                }
            }
            return nftsowned;
            
        }


        /// <summary>
        /// Iterates in the blockchain and sees how much currency the address has.
        /// Purely informational, like <see cref="GetNFTs(string)"/>
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public int GetBalance(string address)
        {
            int balance = 0;

            foreach (Block block in this.Chain) {
                if(block.Transactions is null)
                {
                    continue;
                }
                foreach (var transaction in block.Transactions) {
                    if(transaction.GetType() == typeof(Transaction))
                    {
                        Transaction t;
                        t = (Transaction)transaction;
                        if (t.FromAddress == address)
                        {
                            balance -= t.Amount;
                        }

                        if (t.ToAddress == address)
                        {
                            balance += t.Amount;
                        }
                    }
                    

                }
            }

            return balance;
        }
    }
}
