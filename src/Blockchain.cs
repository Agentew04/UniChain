using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Key = NBitcoin.Key;
using RodrigoChain.Exceptions;

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

            //NFT transfer and burn is an exception to the IsValid rule
            if(e.GetType()==typeof(NFTTransfer)){
                if(((NFTTransfer)e).IsValid(this)){
                    PendingTransactions.Add(e);
                }
                return;
            }
            if(e.GetType()==typeof(NFTBurn)){
                if(((NFTBurn)e).IsValid(this)){
                    PendingTransactions.Add(e);
                }
                return;
            }
            if(e.IsValid()){
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
            if (!block.HasValidTransactions()) 
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


        // /// <summary>
        // /// Searches for a Token/NFT inside the blockchain. The information it returns
        // /// belong WHEN THE TOKEN WAS CREATED. For updated info, use <see cref="GetToken(Guid)"/>.
        // /// Purely informational, like <see cref="GetBalance(string)"/></summary>
        // /// <param name="tokenId">The token id to be looked up for</param>
        // /// <returns>A <see cref="Nullable{NFT}"/>. Returns <see langword="null"/> if no one created a nft like this before</returns>
        // public NFT? GetTokenOrigin(Guid tokenId)
        // {
        //     foreach (Block block in this.Chain)
        //     {
        //         if (block.Transactions is null)
        //         {
        //             continue;
        //         }
        //         foreach (var transaction in block.Transactions)
        //         {
        //             if (typeof(TokenCreation).IsInstanceOfType(transaction))
        //             {
        //                 NFT nft = (NFT)transaction;
        //                 if(nft.TokenId == tokenId)
        //                 {
        //                     return nft;
        //                 }
        //             }
        //             else { continue; }
        //         }
        //     }
        //     return null;
        // }


        // /// <summary>
        // /// Searches for a Token/NFT inside the blockchain. The information it returns
        // /// belong WHEN THE TOKEN WAS CREATED. For updated info, use <see cref="GetToken(Guid)"/>.
        // /// Purely informational, like <see cref="GetBalance(string)"/></summary>
        // /// <param name="timestamp">The timestamp of the <see cref="TokenCreation"/></param>
        // /// <returns>A <see cref="Nullable{NFT}"/>. Returns <see langword="null"/> if no one created a nft like this before</returns>
        // public NFT? GetTokenOrigin(long timestamp)
        // {
        //     foreach (Block block in this.Chain)
        //     {
        //         if (block.Transactions is null)
        //         {
        //             continue;
        //         }
        //         foreach (var transaction in block.Transactions)
        //         {
        //             if (typeof(TokenCreation).IsInstanceOfType(transaction))
        //             {
        //                 if (((NFT)transaction).Timestamp == timestamp)
        //                 {
        //                     return (NFT)transaction;
        //                 }
        //             }else { continue; }
        //         }
        //     }
        //     return null;
        // }


        // /// <summary>
        // /// Searches for a Token/NFT inside the blockchain. The information it returns
        // /// belong WHEN THE TOKEN WAS CREATED. For updated info, use <see cref="GetToken(Guid)"/>.
        // /// Purely informational, like <see cref="GetBalance(string)"/>
        // /// </summary>
        // /// <param name="ownerAddress">The timestamp of the <see cref="TokenCreation"/></param>
        // /// <returns>A <see cref="Nullable{T}"/>. Returns <see langword="null"/> if no one created a nft like this before</returns>
        // public NFT? GetTokenOrigin(string ownerAddress)
        // {
        //     foreach (Block block in this.Chain)
        //     {
        //         if (block.Transactions is null)
        //         {
        //             continue;
        //         }
        //         foreach (var transaction in block.Transactions)
        //         {
        //             if (typeof(TokenCreation).IsInstanceOfType(transaction))
        //             {
        //                 if (((NFT)transaction).Owner == ownerAddress)
        //                 {
        //                     return (NFT)transaction;
        //                 }
        //             }
        //             else { continue; }
        //         }
        //     }
        //     return null;
        // }


        // /// <summary>
        // /// Searches the current information about a Token in the blockchain.
        // /// </summary>
        // /// <param name="tokenId">The GUID/UUID of the token that will be searched</param>
        // /// <returns>Returns a <see cref="NFT"/> representing the current state of the Token</returns>
        // /// <exception cref="TokenNotFoundException"/>
        // public NFT GetToken(Guid tokenId)
        // {
        //     TokenMetadata meta = new("","",new Dictionary<object, object>());
        //     long timestamp = 0;
        //     bool istokenfound = false;
        //     if(GetTokenOrigin(tokenId).HasValue == false || GetTokenOrigin(tokenId) == null) { throw new TokenNotFoundException("This token does not exist"); }
        //     NFT currentstate = new();
        //     foreach (Block block in this.Chain){
        //         foreach(var transaction in block.Transactions){
        //             if (transaction.GetType().IsSubclassOf(typeof(BlockChainEvent))){
        //                 if(((BlockChainEvent)transaction).EventType == EventType.TokenCreation){
        //                     TokenCreation t = (TokenCreation)transaction;
        //                     if (t.TokenId == tokenId)
        //                     {
        //                         meta = t.Metadata;
        //                         timestamp = t.Timestamp;
        //                         istokenfound = true;
        //                     }
        //                 }
        //                 if(((BlockChainEvent)transaction).EventType == EventType.TokenTransaction){
        //                     TokenTransaction t = (TokenTransaction)transaction;
        //                     if(t.TokenId == tokenId)
        //                     {
        //                         currentstate = (NFT)t;
        //                     }
        //                 }
        //             }
        //         }
        //     }
        //     if (!istokenfound) { throw new TokenNotFoundException(); }
        //     currentstate.Metadata = meta;
        //     currentstate.Timestamp = timestamp;
        //     return currentstate;
        // }


        /// <summary>
        /// Searches for the current owner of a Token
        /// </summary>
        /// <returns>The public Address/Key of the current owner</returns>
        // public Address GetTokenOwner(Guid tokenId)
        // {
        //     if (GetTokenOrigin(tokenId).HasValue == false || GetTokenOrigin(tokenId) == null) { throw new TokenNotFoundException("This token does not exist"); }
        //     Address currentowner = GetTokenOrigin(tokenId).Value.Owner;
        //     foreach (Block block in this.Chain)
        //     {
        //         foreach (var transaction in block.Transactions)
        //         {
        //             if (transaction.GetType().IsSubclassOf(typeof(BlockChainEvent)))
        //             {
        //                 if (((BlockChainEvent)transaction).EventType == EventType.TokenTransaction)
        //                 {
        //                     TokenTransaction t = (TokenTransaction)transaction;
        //                     if (t.TokenId == tokenId)
        //                     {
        //                         currentowner = t.ToAddress;
        //                     }

        //                 }
        //             }
        //         }
        //     }
        //     return currentowner;
        // }


        // /// <summary>
        // /// Iterates in the blockchain and sees which NFTs the address has.
        // /// Purely informational, like <see cref="GetBalance(string)"/>
        // /// </summary>
        // /// <param name="address">The address to be looked up</param>
        // /// <returns>A <see cref="IList{NFT}"/> containing all the nfts the user has</returns>
        // public IList<NFT> GetNFTs(string address)
        // {
        //     IList<NFT> nftsowned = new List<NFT>();

        //     //get all nfts
        //     foreach (Block block in this.Chain)
        //     {
        //         if (block.Transactions is null)
        //         {
        //             continue;
        //         }
        //         foreach (var transaction in block.Transactions)
        //         {
        //             if (typeof(TokenCreation).IsInstanceOfType(transaction))
        //             {
        //                 NFT nft = (NFT)transaction;
        //                 if(nft.Owner == address)
        //                 {
        //                     nftsowned.Add(nft);
        //                 }
        //             }
        //             else { continue; }
        //         }
        //     }
        //     return nftsowned;
            
        // }


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
                var q = block.Transactions.AsQueryable()
                .Where(trans=>typeof(Transaction).IsInstanceOfType(trans))
                .Where(trans=>(trans as Transaction).FromAddress==address || (trans as Transaction).ToAddress==address);
                foreach(var q1 in q){
                    Transaction t=(Transaction)q1;
                    if(t.ToAddress==address){
                        balance+=t.Amount;
                    }else{
                        balance-=t.Amount;
                    }
                }
            }
            return balance;
        }

        #endregion
    }
}
