using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SHA3.Net;

namespace Unichain.Core;

/// <summary>
/// A class representing a Public Key, normally derived from a <see cref="PrivateKey"/>
/// </summary>
public class PublicKey {

    /// <summary>
    /// An array of bytes containing the key.
    /// </summary>
    public byte[] Key { get; init; }

    /// <summary>
    /// Initializes a PublicKey from an existing key
    /// </summary>
    /// <param name="bytes"></param>
    public PublicKey(byte[] bytes) {
        Key = bytes;
    }
    
    /// <summary>
    /// Initializes a new public key from a private key
    /// </summary>
    /// <param name="privateKey">The private key used to derive the public key</param>
    public PublicKey(PrivateKey privateKey) {
        Key = privateKey.DerivePublicKeyBytes();
    }

    /// <summary>
    /// Derives a address from this public key.
    /// </summary>
    /// <returns>A string containing the address created</returns>
    public string DeriveAddress() {
        using var sha = Sha3.Sha3256();
        var hash = sha.ComputeHash(Key);
        var lastbytes = hash.TakeLast(20).ToArray();
        return $"0x{Convert.ToHexString(lastbytes)}";
    }

    /// <summary>
    /// Verifies that the data was signed by this public key and the signature match.
    /// </summary>
    /// <param name="s">A string containing the data</param>
    /// <param name="signature">A string with the signature</param>
    /// <returns>A <see cref="bool"/> representing if the verification was successful or not.</returns>
    public bool Verify(string s, string signature) {
        var bytes = Encoding.UTF8.GetBytes(s);
        var signatureBytes = Convert.FromBase64String(signature);
        return Verify(bytes, signatureBytes);
    }

    /// <summary>
    /// Verifies that the data was signed by this public key and the signature match.
    /// </summary>
    /// <param name="data">An array of bytes with the original data</param>
    /// <param name="signature">An array of bytes containing the signature</param>
    /// <returns>A <see cref="bool"/> representing if the verification was successful or not.</returns>
    public bool Verify(byte[] data, byte[] signature) {
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportSubjectPublicKeyInfo(Key, out _);
        return ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA512);
    }

    public override string ToString() => Convert.ToHexString(Key);
}
