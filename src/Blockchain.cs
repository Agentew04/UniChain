using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Key = NBitcoin.Key;
using RodrigoChain.Exceptions;
using System.Linq.Expressions;
using RodrigoChain.Core;
using RodrigoChain.Events;

namespace RodrigoChain
{
    public class Blockchain
    {
        /// <summary>
        /// Represents the Chain of <see cref="Block"/> for this blockchain
        /// </summary>
        public IList<Block> Chain { get; private set; }
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
        private IList<BaseBlockChainEvent> PendingTransactions = new List<BaseBlockChainEvent>();



        public Blockchain()
        {
            Chain = new List<Block>
            {
                new Block(null, null)
            };
        }

        public void PrintChain(){
            Console.WriteLine(JsonConvert.SerializeObject(this.Chain));
        }

        /// <summary>
        /// Returns the latest block in the chain
        /// </summary>
        /// <returns>The latest block</returns>
        private Block GetLatestBlock()
        {
            return Chain[Chain.Count - 1];
        }

        #region add to chain

        /// <summary>
        /// Adds a new event to the pending transactions list
        /// /// </summary>
        public void AddEvent(BaseBlockChainEvent e){
            //special checks
            if(e.GetType() == typeof(Transaction)){
                if(((Transaction)e).FromAddress=="network"){
                    throw new InvalidTransactionException("Cannot add a transaction from the network");
                }
                if(((Transaction)e).ToAddress=="network"){
                    throw new InvalidTransactionException("Cannot add a transaction to the network");
                }
            }


            //add to the pending transactions
            if(e.IsValid(this)){
                PendingTransactions.Add(e);
            }
        }
        public void AddEvents(params BaseBlockChainEvent[] events){
            foreach(var e in events){
                AddEvent(e);
            }
        }

        #endregion

        /// <summary>
        /// Checks all the transactions in <see cref="PendingTransactions"/> and creates a new block
        /// in the blockchain
        /// </summary>
        /// <param name="minerAddress">The address that will receive all the rewards</param>
        public void MinePendingTransactions(Address minerAddress)
        {
            if (minerAddress.IsNull()) { throw new ArgumentNullException("The miner address is null!",new NullAddressException()); }
            PendingTransactions.Insert(0, new Transaction(new User(true), minerAddress, this.Reward));
            Block block = new(GetLatestBlock().Hash, PendingTransactions);
            if (!block.HasValidTransactions(this)) 
            { 
                throw new InvalidTransactionException("One of the transactions is not valid!"); 
            }

            Block latestBlock = GetLatestBlock();
            block.Index = latestBlock.Index + 1;
            block.PreviousHash = latestBlock.Hash;
            block.MineBlock(this.Difficulty);
            Chain.Add(block);

            PendingTransactions = new List<BaseBlockChainEvent>();
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


        #region find in the chain

        public IEnumerable<Pool> GetPools(){
            var q = from block in Chain
                    from transaction in block.Transactions
                    where transaction.GetType() == typeof(PoolVote)
                    select (PoolVote)transaction;

            foreach(var block in Chain){
                if(block.Transactions==null){
                    continue;
                }
                foreach(var e in block.Transactions){
                    if(e.GetType() == typeof(PoolOpen)){
                        //get all votes in this pool
                        var votes = q.Where(x => x.PoolId == ((PoolOpen)e).PoolId);
                        yield return Pool.Parse((PoolOpen)e,votes);
                    }
                }
            }
        }
        public Pool GetPoolById(Guid poolId){
            return GetPools().Where(p => p.PoolId == poolId).First();
        }

        /// <summary>
        /// Searches for a Token/NFT inside the blockchain. The information it returns
        /// belong WHEN THE TOKEN WAS CREATED. For updated info, use <see cref="GetNFTTransfer(Guid)"/>.
        /// Purely informational, like <see cref="GetBalance(string)"/></summary>
        /// <param name="NFTId">The token id to be looked up for</param>
        /// <returns>A <see cref="Nullable{NFT}"/>. Returns <see langword="null"/> if no one created a nft like this before</returns>
        public NFTMint GetNFTMint(Guid NFTId)
        {
            var q = from block in Chain
                    where block.Transactions != null
                    from transaction in block.Transactions
                    where transaction.GetType() == typeof(NFTMint)
                    select (NFTMint)transaction;
            if(q==null || q.Count()==0){
                return null;
            }
            foreach (var ntfmint in q){
                if(ntfmint.NFTId==NFTId){
                    return ntfmint;
                }else{
                    continue;
                }
            }
            return null;
        }

        /// <summary>
        /// Searches the current information about a Token in the blockchain.
        /// </summary>
        /// <param name="tokenId">The GUID/UUID of the token that will be searched</param>
        /// <returns>Returns a NFTTransfer or null if not found</returns>
        public NFTTransfer GetNFTTransfer(Guid tokenId)
        {
            var q = from block in Chain
                    where block.Transactions != null
                    from transaction in block.Transactions
                    where transaction.GetType() == typeof(NFTTransfer)
                    where ((NFTTransfer)transaction).NFTId == tokenId
                    select (NFTTransfer)transaction;
            if(q==null || q.Count()==0){
                return null;
            }
            return q.Last();
        }


        /// <summary>
        /// Searches for the current owner of a Token
        /// </summary>
        /// <returns>The public Address of the current owner</returns>
        public Address GetCurrentNFTOwner(Guid tokenId)
        {
            var nftTransfer = GetNFTTransfer(tokenId);
            if(nftTransfer==null){
                return null;
            }
            return nftTransfer.ToAddress;
        }

        /// <summary>
        /// Iterates in the blockchain and sees how much currency the address has.
        /// </summary>
        /// <param name="address">The Address to be lookedup for</param>
        /// <returns>The amount of money that this Address has</returns>
        public int GetBalance(Address address)
        {
            int balance = 0;

            //check if null
            if (address.IsNull()) { throw new ArgumentNullException("The address is null!", new NullAddressException()); }

            //get all transaction with this address
            var q = from block in Chain
                    where block.Transactions != null
                    from transaction in block.Transactions
                    where transaction.GetType() == typeof(Transaction)
                    where ((Transaction)transaction).FromAddress == address || ((Transaction)transaction).ToAddress == address
                    select (Transaction)transaction;
                    
            //check if the user has made any transactions
            if (q == null || q.Count()==0) { return 0; }

            foreach (var tx in q)
            {
                if(tx==null){
                    continue;
                }
                //if the transaction is sending money to this address
                if (tx.ToAddress == address)
                {
                    //add the amount to the balance
                    balance += tx.Amount;
                    continue;
                }

                //if the transaction is receiving money from this address
                if (tx.FromAddress == address)
                {
                    //subtract the amount from the balance
                    balance -= tx.Amount;
                    continue;
                }

            }
            return balance;
        }

        #endregion
    }
}
