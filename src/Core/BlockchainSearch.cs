using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unichain.Core;
using Unichain.Events;
using Unichain.Exceptions;

namespace Unichain.Core;

public partial class Blockchain{

    /// <summary>
    /// Finds all transactions that match <paramref name="predicate"/> and are of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type that will be looked up for in the blockchain</typeparam>
    /// <param name="predicate">The expression used to filter transactions in the <see cref="Chain"/></param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the results</returns>
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

    /// <summary>
    /// Finds all transactions that match <paramref name="predicate"/>. This function assumes T is a <see cref="ITransaction"/> and
    /// makes no type checks.
    /// </summary>
    /// <param name="predicate">The expression used to filter transactions in the <see cref="Chain"/></param>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the results</returns>
    public IEnumerable<ITransaction> Find(Expression<Func<ITransaction, bool>> predicate) {
        //find the objects that match T inside each block in this.Chain
        var result = Chain.Where(block => block is not null && block.Events is not null && block.Events.Any())
            .SelectMany(block => block.Events)
            .Where(e => e is not null)
            .Where(e => predicate.Compile().Invoke(e));
        return result;
    }

    /// <summary>
    /// Finds all transaction of type <typeparamref name="T"/> in the <see cref="Chain"/>. 
    /// </summary>
    /// <typeparam name="T">The type of transaction to be filtered. If this is <see cref="ITransaction"/>, this
    /// will select all transactions of the chain.</typeparam>
    /// <returns>An <see cref="IEnumerable{T}"/> containing the results</returns>
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
    public IEnumerable<PoolCreate> GetPools()
    {
        var pools = FindAll<PoolCreate>();
        return pools;
    }

    /// <summary>
    /// Returns a <see cref="PoolCreate"/> event by its unique id.
    /// </summary>
    /// <param name="poolId">The id that its going to be searched</param>
    /// <returns>The <see cref="PoolCreate"/> event, <see langword="null"/> if not found.</returns>
    public PoolCreate? GetPoolById(Guid poolId)
    {
        var pool = Find<PoolCreate>(x => x.PoolId == poolId).FirstOrDefault();
        return pool;
    }

    /// <summary>
    /// Finds all
    /// </summary>
    /// <param name="poolId">The id that its going to be searched</param>
    /// <returns>A list with the number of votes</returns>
    /// <exception cref="Exception"></exception>
    public List<int> GetVotes(Guid poolId)
    {
        var poolOpen = GetPoolById(poolId);
        if(poolOpen is null)
            // todo create a custom exception. This message only shows at runtime, not useful for devs(docs)
            throw new Exception("Pool not found");
        
        var options = poolOpen.Options.Count();
        var poolVotes = Find<PoolVote>(x => x.PoolId == poolId);

        List<int> votes = Enumerable.Repeat(0, options).ToList();

        for (int i = 0; i < poolOpen.Options.Count(); i++) {
            votes[i] += poolVotes.Where(x => x.OptionSelected == i).Count();
        }
        return votes;
    }

    /// <summary>
    /// Get which option aAddress voted.
    /// </summary>
    /// <param name="poolId">The unique poolId that its going to be searched</param>
    /// <param name="voter">The address to be looked up for</param>
    /// <returns>The index of the option voted or -1 if the addres doesn't voted in this pool.</returns>
    public int GetAddressOptionInPool(Guid poolId, string address)
    {
        var vote = Find<PoolVote>(poolvote => poolvote.PoolId == poolId)
            .Where(x => x.Actor.Address == address)
            .FirstOrDefault();

        if(vote is null)
            return -1;

        return vote.OptionSelected;
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

        var finalBalance = (currencyReceived + currencyMined) - (currencySent + totalFees);
        return finalBalance;
    }

    #endregion
}