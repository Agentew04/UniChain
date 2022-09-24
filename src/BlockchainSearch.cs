using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unichain.Core;
using Unichain.Events;
using Unichain.Exceptions;

namespace Unichain
{
    public partial class Blockchain
    {

        public IEnumerable<T> Find<T>(Expression<Func<T, bool>> predicate) where T : ITransaction {
            //find the objects that match T inside each block in this.Chain
            var result = Chain.Where(block => block is not null && block.Events is not null && block.Events.Any())
                .SelectMany(block => block.Events)
                .Where(e => e is not null)
                .Where(e => e is T)
                .Where(e => predicate.Compile().Invoke((T)e) )
                .Select(e => (T)e);
            //foreach (var block in this.Chain)
            //{
            //    if (block == null || block.Events == null)
            //    {
            //        continue;
            //    }
            //    var events = from e in block.Events
            //                 where e != null
            //                 where e.GetType() == typeof(T)
            //                 where predicate.Compile().Invoke((T)e)
            //                 select (T)e;
            //    result = result.Concat(events);
            //}
            return result;
        }

        public IEnumerable<ITransaction> Find(Expression<Func<ITransaction, bool>> predicate) {
            //find the objects that match T inside each block in this.Chain
            var result = Chain.Where(block => block is not null && block.Events is not null && block.Events.Any())
                .SelectMany(block => block.Events)
                .Where(e => e is not null)
                .Where(e => predicate.Compile().Invoke(e));
            return result;
        }

            public IEnumerable<T> FindAll<T>() where T : ITransaction {
            //find the objects that match T inside each block in this.Chain
            var events = Chain.Where(block => block is not null && block.Events is not null && block.Events.Any())
                .SelectMany(block => block.Events)
                .Where(e => e is not null)
                .Where(e => e is T)
                .Select(e => (T)e);
            return events;
        }

        #region pools

        /// <summary>
        /// Returns all PoolOpen events
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PoolOpen> GetPools()
        {
            var pools = FindAll<PoolOpen>();
            return pools;
        }

        /// <summary>
        /// Returns a PoolOpen event by its id
        /// </summary>
        /// <param name="poolId">The id that its going to be searched</param>
        /// <returns>The PoolOpenEvent, containing Metadata</returns>
        public PoolOpen GetPoolById(Guid poolId)
        {
            var pool = Find<PoolOpen>(x => x.PoolId == poolId).FirstOrDefault();
            return pool;
        }

        /// <summary>
        /// Returns a list of votes in the Pool by its ID. List index follow Metadata option index
        /// </summary>
        /// <param name="poolId">The id that its going to be searched</param>
        /// <returns>A list with the number of votes</returns>
        public List<int> GetVotes(Guid poolId)
        {
            //vars
            var poolOpen = GetPoolById(poolId);
            var poolVotes = Find<PoolVote>(x => x.PoolId == poolId);

            //initialize list
            List<int> votes = new();
            for (int i = 0; i < poolOpen.Metadata.Options.Length; i++)
            {
                votes.Add(0);
            }

            //count votes
            foreach (var vote in poolVotes)
            {
                votes[vote.VoteIndex]++;
            }

            return votes;
        }

        /// <summary>
        /// Returns the total number of votes in the Pool by its ID
        /// </summary>
        /// <param name="poolId">The id that its going to be searched</param>
        /// <returns></returns>
        public int GetTotalVotes(Guid poolId)
        {
            var votes = GetVotes(poolId);
            return votes.Sum();
        }

        /// <summary>
        /// Returns the number of votes in one option by its ID
        /// </summary>
        /// <param name="poolId">The id that its going to be searched</param>
        /// <param name="optionIndex">The index of the option to be searched</param>
        /// <returns></returns>
        public int GetVote(Guid poolId, int optionIndex)
        {
            var votes = GetVotes(poolId);
            return votes[optionIndex];
        }

        /// <summary>
        /// Get which option one Address voted
        /// </summary>
        /// <param name="poolId">The id that its going to be searched</param>
        /// <param name="voter">The Address to be looked up for</param>
        /// <returns>The index of the option voted</returns>
        public int GetVoterOption(Guid poolId, string voter)
        {
            var votes = GetVotes(poolId);
            var poolvote = Find<PoolVote>(poolvote => poolvote.PoolId == poolId && poolvote.VoterAddress == voter).FirstOrDefault();
            if (poolvote == null)
            {
                return -1;
            }
            return poolvote.VoteIndex;
        }
        #endregion

        #region nft
        
        /// <summary>
        /// Checks if the NFT is owned by the Address
        /// </summary>
        /// <param name="nftId">The Id to be searched</param>
        /// <param name="owner">The Address to be searched</param>
        /// <returns>The result of the operation</returns>
        public bool IsNFTOwner(Guid nftId, string owner)
        {
            var address = GetNFTOwner(nftId);
            return address == owner;
        }

        /// <summary>
        /// Gets current owner of this nft
        /// </summary>
        /// <param name="nftId">The id to be searched</param>
        /// <returns>The address of the current owner</returns>
        /// <exception cref="NFTNotFoundException">Thrown when a NFT is not found</exception>
        public string GetNFTOwner(Guid nftId)
        {
            //get NFTMint
            var nftMint = Find<NFTMint>(x => x.NFTId == nftId).FirstOrDefault();

            //has not been minted
            if (nftMint is null)
                throw new NFTNotFoundException();

            //get last NFTTransfer
            var lastTransfer = Find<NFTTransfer>(x => x.NFTId == nftId).LastOrDefault();

            if (lastTransfer is null)
                return nftMint.Actor.Address;
            return lastTransfer.ToAddress;
        }

        /// <summary>
        /// Gets the metadata associated with an NFT unique id.
        /// </summary>
        /// <param name="nftId">The id to be searched</param>
        /// <returns>An NFT object</returns>
        /// <exception cref="NFTNotFoundException">Thrown when a NFT is not found</exception>
        public Dictionary<string, object> GetNFTMetadata(Guid nftId)
        {
            //get NFTMint
            var nftMint = Find<NFTMint>(x => x.NFTId == nftId).FirstOrDefault();
            
            if (nftMint is null)
                throw new NFTNotFoundException();
            
            return nftMint.Metadata;
        }

        /// <summary>
        /// Checks if this NFT has already been minted
        /// </summary>
        /// <param name="nftId">The unique id of the NFT</param>
        /// <returns></returns>
        public bool IsNFTMinted(Guid nftId) => Find<NFTMint>(x => x.NFTId == nftId).Any();

        /// <summary>
        /// Checks if this NFT has been burned
        /// </summary>
        /// <param name="nftId">The unique id of the NFT</param>
        /// <returns></returns>
        public bool IsNFTBurned(Guid nftId) => Find<NFTBurn>(x => x.NFTId == nftId).Any();

        #endregion

        #region money

        /// <summary>
        /// Gets the balance of an address
        /// </summary>
        /// <param name="address">The address to be checked</param>
        /// <returns>The amount of money this user has</returns>
        public double GetBalance(string address)
        {
            var totalFees = Find(x => x.Actor.Address == address).Sum(x => x.Fee);
            var currencyReceived = Find<CurrencyTransaction>(x => x.ToAddress == address).Sum(x => x.Amount);
            var currencySent = Find<CurrencyTransaction>(x => x.Actor.Address == address).Sum(x => x.Amount);
            var currencyMined = Chain.Where(b => b.Miner == address).Sum(b => Reward + b.CollectedFees);

            var finalBalance = (currencyReceived + currencyMined ?? 0) - (currencySent + totalFees);
            return finalBalance;
        }

        #endregion
    }
}