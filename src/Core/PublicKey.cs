using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
    /// An array of bytes containing the public key, formatted for Bouncy Castle API.
    /// </summary>
    public byte[] BC_Key { get; init; }

    /// <summary>
    /// The accuracy used for checksum addresses. Should be adapted as user count grows.
    /// Should not interfere with blockchain at all, just for safety.
    /// </summary>
    private const int accuracy = 4;

    /// <summary>
    /// Initializes a PublicKey from an existing key
    /// </summary>
    /// <param name="bytes"></param>
    public PublicKey(byte[] bytes, byte[] bc_bytes) {
        Key = bytes;
        BC_Key = bc_bytes;
    }
    
    /// <summary>
    /// Initializes a new public key from a private key
    /// </summary>
    /// <param name="privateKey">The private key used to derive the public key</param>
    public PublicKey(PrivateKey privateKey) {
        (Key, BC_Key) = privateKey.DerivePublicKeyBytes();
    }

    /// <summary>
    /// Derives a address from this public key. 
    /// Is 42 characters long without checksum and 50 with (including '0x' prefix).
    /// </summary>
    /// <returns>A string containing the address created</returns>
    public string DeriveAddress(bool includeChecksum = true) {
        using var sha3 = Sha3.Sha3256();
        var hash = sha3.ComputeHash(Key);
        var addrBytes = hash.TakeLast(20).ToArray();
        if(!includeChecksum)
            return $"0x{Convert.ToHexString(addrBytes)}";
        
        var checksum = CalculateChecksum(addrBytes, PublicKey.accuracy);
        var address = addrBytes.Concat(checksum).ToArray();
        return $"0x{Convert.ToHexString(address)}";
    }

    /// <summary>
    /// Calculates the checksum for a given address.
    /// </summary>
    /// <param name="address">The address bytes('0x' not included!)</param>
    /// <param name="accuracy">The amount of chars in the checksum</param>
    /// <returns>A <see cref="byte[]"/> containing only the checksum bytes</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the accuracy is not in bounds: [2,8] and must be even.</exception>
    public static byte[] CalculateChecksum(byte[] address, int accuracy ) {
        if (accuracy < 2 || accuracy > 8 || accuracy % 2 != 0)
            throw new ArgumentOutOfRangeException(nameof(accuracy),"Checksum accuracy must be between [2,8] and be even");
        
        // 8 accuracy = 8 chars = 4 bytes
        // 6 accuracy = 6 chars = 3 bytes
        // 4 accuracy = 4 chars = 2 bytes
        // 2 accuracy = 2 chars = 1 byte
        
        using var sha = SHA256.Create();
        var addrhash = sha.ComputeHash(address.Take(20).ToArray());
        return addrhash.Take(accuracy/2).ToArray(); // 8 acur => 4 bytes => 8 chars
    }

    /// <summary>
    /// Checks if an address is valid and if it has and checksum, checks it.
    /// </summary>
    /// <param name="address">The address to be verified, with or without checksum</param>
    /// <exception cref="FormatException">Thrown if the string is malformatted, for example:
    /// odd number of digits on checksum</exception>
    /// <returns>A <see cref="bool"/> with the result.</returns>
    public static bool IsAddressValid(string address) {
        // Regex to check valid address.  
        Regex regex = new("^0x([0-9a-fA-F]{40})([0-9a-fA-F]{0,8})$");
        Match match = regex.Match(address);
        if (match.Success) {
            var groups = match.Groups;
            // groups[0] its the match itself
            var addressBytes = Convert.FromHexString(groups[1].Value);
            var readSum = groups.Count >= 3 && groups[2].Length > 0
                ? Convert.FromHexString(groups[2].Value) : null;
            if (readSum is null)
                return true;

            var calculatedSum = CalculateChecksum(addressBytes, readSum.Length * 2);
            if (readSum.SequenceEqual(calculatedSum))
                return true;
            
        }
        return false;
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
