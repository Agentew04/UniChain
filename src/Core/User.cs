using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Unichain.Core;

/// <summary>
/// Represents a user doing actions in the BlockChain
/// </summary>
public class User
{
    public string Address { get; init; }
    
    [Newtonsoft.Json.JsonIgnore]
    public PrivateKey? PrivateKey { get; init; }
    public PublicKey PublicKey { get; init; }
    
    /// <summary>
    /// Initializes a user with a random set of keys and address
    /// </summary>
    public User()
    {
        PrivateKey = new PrivateKey();
        PublicKey = PrivateKey.DerivePublicKey();
        Address = PublicKey.DeriveAddress();
    }

    /// <summary>
    /// Initializes a user with all properties filled
    /// </summary>
    /// <param name="privateKey">This user's private key</param>
    public User(PrivateKey privateKey)
    {
        PrivateKey = privateKey;
        PublicKey = PrivateKey.DerivePublicKey();
        Address = PublicKey.DeriveAddress();
    }

    /// <summary>
    /// Initializes a user without a private key
    /// </summary>
    /// <param name="publicKey">The public key associated with this user</param>
    public User(PublicKey publicKey) {
        PrivateKey = null;
        PublicKey = publicKey;
        Address = PublicKey.DeriveAddress();
    }

    /// <summary>
    /// Signs the given string with the current Private Key
    /// </summary>
    /// <param name="str">The string that will be signed</param>
    /// <returns>The signature in base64</returns>
    public string SignString(string str)
    {
        if(PrivateKey is null)
            throw new InvalidOperationException("PrivateKey is null for user " + Address);
        return PrivateKey.Sign(str);
    }
    /// <summary>
    /// Verifies if the message matches the given signature
    /// </summary>
    /// <param name="message">The original message</param>
    /// <param name="signature">The base64 signature</param>
    /// <returns>A <see cref="bool"/> with the result</returns>
    public bool VerifySignature(string message, string signature)
    {
        return PublicKey.Verify(message, signature);
    }

    public override bool Equals(object? obj)
    {
        return obj is User user && PublicKey.Equals(user.PublicKey)
            && Address.Equals(user.Address);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Address, PrivateKey);
    }
    
    public static bool operator ==(User a, User b) => a.Equals(b);
    public static bool operator !=(User a, User b) => !a.Equals(b);
}