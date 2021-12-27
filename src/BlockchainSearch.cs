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

        public IEnumerable<T> Find<T>(Expression<Func<T, bool>> predicate) where T : BaseBlockChainEvent
        {
            //find the objects that match T inside each block in this.Chain
            foreach (var block in this.Chain)
            {
                if (block == null)
                {
                    continue;
                }
                if (block.Events == null)
                {
                    continue;
                }
                foreach (var obj in block.Events)
                {
                    if (obj is T)
                    {
                        var t = (T)obj;
                        if (predicate.Compile().Invoke(t))
                        {
                            yield return t;
                        }
                    }
                }
            }
        }
        public IEnumerable<T> FindAll<T>() where T : BaseBlockChainEvent
        {
            //find the objects that match T inside each block in this.Chain
            foreach (var block in this.Chain)
            {
                if (block == null)
                {
                    continue;
                }
                if (block.Events == null)
                {
                    continue;
                }
                foreach (var obj in block.Events)
                {
                    if (obj is T)
                    {
                        var t = (T)obj;
                        yield return t;
                    }
                }
            }
        }
        #region pools

        /// <summary>
        /// Returns all PoolOpen events
        /// </summary>
        /// <returns></returns>
        public IEnumerable<PoolOpen> GetPools(){
            var pools = FindAll<PoolOpen>();
            return pools;
        }

        /// <summary>
        /// Returns a PoolOpen event by its id
        /// </summary>
        /// <param name="poolId">The id that its going to be searched</param>
        /// <returns>The PoolOpenEvent, containing Metadata</returns>
        public PoolOpen GetPoolById(Guid poolId){
            var pool = Find<PoolOpen>(x => x.PoolId == poolId).FirstOrDefault();
            return pool;
        }

        /// <summary>
        /// Returns a list of votes in the Pool by its ID. List index follow Metadata option index
        /// </summary>
        /// <param name="poolId">The id that its going to be searched</param>
        /// <returns>A list with the number of votes</returns>
        public List<int> GetVotes(Guid poolId){
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
            foreach(var vote in poolVotes){
                votes[vote.VoteIndex]++;
            }

            return votes;
        }   
        
        /// <summary>
        /// Returns the total number of votes in the Pool by its ID
        /// </summary>
        /// <param name="poolId">The id that its going to be searched</param>
        /// <returns></returns>
        public int GetTotalVotes(Guid poolId){
            var votes = GetVotes(poolId);
            return votes.Sum();
        }

        /// <summary>
        /// Returns the number of votes in one option by its ID
        /// </summary>
        /// <param name="poolId">The id that its going to be searched</param>
        /// <param name="optionIndex">The index of the option to be searched</param>
        /// <returns></returns>
        public int GetVote(Guid poolId, int optionIndex){
            var votes = GetVotes(poolId);
            return votes[optionIndex];
        }
        
        /// <summary>
        /// Get which option one Address voted
        /// </summary>
        /// <param name="poolId">The id that its going to be searched</param>
        /// <param name="voter">The Address to be looked up for</param>
        /// <returns>The index of the option voted</returns>
        public int GetVoterOption(Guid poolId, Address voter){
            var votes = GetVotes(poolId);
            var poolvote = Find<PoolVote>(poolvote => poolvote.PoolId == poolId && poolvote.VoterAddress == voter).FirstOrDefault();
            if(poolvote == null){
                return -1;
            }
            return poolvote.VoteIndex;
        }
        #endregion

        #region nft

        /// <summary>
        /// Gets the current owner of the NFT
        /// </summary>
        /// <param name="nftId">The id to be searched</param>
        /// <returns>A tuple with the current owner and if it has been burned of not</returns>
        public (Address,bool) getCurrentNFTOwner(Guid nftId){
            //get NFTMint
            var nftMint = Find<NFTMint>(x => x.NFTId == nftId).FirstOrDefault();

            //has not been minted
            if(nftMint == null){
                throw new NFTNotFoundException();
            }

            //get last NFTTransfer
            var nftTransfer = Find<NFTTransfer>(x => x.NFTId == nftId).OrderByDescending(x => x.Timestamp).FirstOrDefault();

            //check for NFTBurn
            var nftBurn = Find<NFTBurn>(x => x.NFTId == nftId).FirstOrDefault();

            //has been burned
            if(nftBurn != null){
                return (nftBurn.BurnerAddress,true);
            }

            //has been transferred
            if(nftTransfer != null){
                return (nftTransfer.ToAddress,false);
            }

            //has not been transferred
            return (nftMint.Owner,false);
        }

        /// <summary>
        /// Checks if the NFT is owned by the Address
        /// </summary>
        /// <param name="nftId">The Id to be searched</param>
        /// <param name="owner">The Address to be searched</param>
        /// <returns>The result of the operation</returns>
        public bool isNFTOwner(Guid nftId, Address owner){
            var (address,_) = getCurrentNFTOwner(nftId);
            return address == owner;
        }

        /// <summary>
        /// Gets all actual and previous owners of this NFT
        /// </summary>
        /// <param name="nftId">The id to be searched</param>
        /// <returns>An IEnumerable containing no duplicates</returns>
        public IEnumerable<Address> GetNFTOwners(Guid nftId){
            //get NFTMint
            var nftMint = Find<NFTMint>(x => x.NFTId == nftId).FirstOrDefault();
            
            //has not been minted
            if(nftMint == null){
                throw new NFTNotFoundException();
            }

            //get all NFTTransfers
            var nftTransfers = Find<NFTTransfer>(x => x.NFTId == nftId);

            var transferOwners = nftTransfers.Select(x => x.ToAddress).Distinct();
            var allOwners = new List<Address>(){nftMint.Owner}.Concat(transferOwners).Distinct();
            return allOwners;
        }

        /// <summary>
        /// Gets all NFTs Owned by an Address
        /// </summary>
        /// <param name="owner">The id to be searched</param>
        /// <returns>An IEnumerable of NFT objects</returns>
        public IEnumerable<NFT> GetNFTsOwned(Address owner){
            //get all NFTMints
            var nftMints = Find<NFTMint>(x => x.Owner == owner);
            foreach (var nftMint in nftMints)
            {
                //get last transfer
                var nftTransfer = Find<NFTTransfer>(x => x.NFTId == nftMint.NFTId).OrderByDescending(x => x.Timestamp).FirstOrDefault();
                //check if is owned by the Address
                if(nftTransfer != null){
                    if(nftTransfer.ToAddress==owner){
                        yield return new NFT(nftMint,null);
                    }else{
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Gets an NFT by its ID
        /// </summary>
        /// <param name="nftId">The id to be searched</param>
        /// <returns>An NFT object</returns>
        public NFT GetNFT(Guid nftId){
            //get NFTMint
            var nftMint = Find<NFTMint>(x => x.NFTId == nftId).FirstOrDefault();

            //has not been minted
            if(nftMint == null){
                throw new NFTNotFoundException();
            }

            //get all NFTTransfers
            var nftTransfers = Find<NFTTransfer>(x => x.NFTId == nftId).ToList();

            var nft = new NFT(nftMint,nftTransfers);
            return nft;
        }

        /// <summary>
        /// Checks if this NFT has been minted or
        /// </summary>
        /// <param name="nftId"></param>
        /// <returns></returns>
        public bool IsNFTMinted(Guid nftId){
            var nftMint = Find<NFTMint>(x => x.NFTId == nftId).FirstOrDefault();
            return nftMint != null;
        }

        /// <summary>
        /// Checks if this NFT has been burned
        /// </summary>
        /// <param name="nftId"></param>
        /// <returns></returns>
        public bool IsNFTBurned(Guid nftId){
            var nftBurn = Find<NFTBurn>(x => x.NFTId == nftId).FirstOrDefault();
            return nftBurn != null;
        }

        #endregion

        #region money

        /// <summary>
        /// Gets the balance of an address
        /// </summary>
        /// <param name="address">The address to be checked</param>
        /// <returns></returns>
        public double GetBalance(Address address)
        {
            var transactions = Find<Transaction>(x => x.FromAddress == address || x.ToAddress==address);
            List<double> amounts = new();
            if(transactions == null || transactions.Count() == 0){
                return 0;
            }
            foreach(var transaction in transactions){
                //check if null
                if(transaction == null){
                    continue;
                }
                if(transaction.FromAddress == address){
                    amounts.Add(-transaction.Amount);
                }else{
                    amounts.Add(transaction.Amount);
                }
            }
            return amounts.Sum();
        }


        /// <summary>
        /// Checks if the Address has more ou the same amount of money
        /// </summary>
        /// <param name="address"></param>
        /// <param name="amount"></param>
        /// <returns></returns>        
        public bool HasEnoughBalance(Address address, double amount){
            var balance = GetBalance(address);
            return balance >= amount;
        }

        #endregion
    }
}