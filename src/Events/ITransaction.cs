using JsonSubTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Unichain.Core;
using Unichain.Exceptions;

namespace Unichain.Events;

// json knownSubType and partial is needed for outsiders add their own transactions to this list
[JsonConverter(typeof(JsonSubtypes), nameof(TypeId))]
[JsonSubtypes.KnownSubType(typeof(CurrencyTransaction), "transaction.currency")]
[JsonSubtypes.KnownSubType(typeof(NFTMint), "transaction.nft.mint")]
[JsonSubtypes.KnownSubType(typeof(NFTTransfer), "transaction.nft.transfer")]
[JsonSubtypes.KnownSubType(typeof(NFTBurn), "transaction.nft.burn")]
[JsonSubtypes.KnownSubType(typeof(PoolCreate), "transaction.pool.create")]
[JsonSubtypes.KnownSubType(typeof(PoolVote), "transaction.pool.vote")]
[JsonSubtypes.KnownSubType(typeof(MessageSendUser), "transaction.msg.send")]
public partial interface ITransaction
{
    #region Variables

    /// <summary>
    /// The User that triggered this transaction, PrivateKey is not included in serialization
    /// </summary>
    public User Actor { get; set; } // addr and pubKey
    
    /// <summary>
    /// The fee that was paid for the miners
    /// </summary>
    public double Fee { get; set; }

    /// <summary>
    /// The time when the object <see cref="ITransaction"/> was created.
    /// Isn't the time that is added to the block. Recommended use of <see cref="DateTime.UtcNow"/>.Ticks
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// A string used to identify which type of transaction this is in the deserializer
    /// </summary>
    public string TypeId { get; set; }

    /// <summary>
    /// The hash signed with the Address/Public Key, it isn't included in the hash but is included in the blockchain
    /// </summary>
    public string? Signature { get; set; }

    // HASH IS NOT A VARIABLE BECAUSE IS CALCULATED AT RUNTIME!

    #endregion

    #region Methods
    
    /// <summary>
    /// Signs the current transaction hash and stores the result in <see cref="Signature"/>
    /// </summary>
    /// <param name="key">If this is null, uses <see cref="Actor"/> to sign, else use the object passed.</param>
    public void SignTransaction(PrivateKey? key = null);

    /// <summary>
    /// Performs checks in the blockchain, checks signature validity and checks if the user has
    /// enough balance to pay the fee.
    /// </summary>
    /// <param name="blockchain">The blockchain reference to check for past activities</param>
    /// <returns>A <see cref="bool"/> representing the result</returns>
    public bool IsValid(Blockchain blockchain);

    /// <summary>
    /// Calculates the hash using at least the address of the actor and the timestamp. Should contain more
    /// information based on which type of transaction it is.
    /// </summary>
    /// <returns>A string with the hash encoded in hexadecimal with no '0x' prefix.</returns>
    public string CalculateHash();
    
    #endregion
}
