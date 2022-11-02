using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Unichain.Core;

/// <summary>
/// A class representing a user's private key for signing and authorizing actions
/// </summary>
public class PrivateKey : IDisposable
{
    public static ECCurve Curve { get; } = ECCurve.NamedCurves.nistP256;
    private readonly ECDsa ecdsa;
    
    /// <summary>
    /// An array containing the key bytes.
    /// </summary>
    public byte[] Key { get; init; }

    /// <summary>
    /// Generates a random PrivateKey using <see cref="ECDsa"/> and <see cref="ECCurve.NamedCurves.nistP256"/> curve.
    /// </summary>
    public PrivateKey()
    {
        ecdsa = ECDsa.Create(Curve);
        Key = ecdsa.ExportECPrivateKey();
    }
    
    /// <summary>
    /// Initializes a PrivateKey from an existing key
    /// </summary>
    /// <param name="key">The bytes of the key in the ECPrivateKey structure</param>
    public PrivateKey(byte[] key) {
        ecdsa = ECDsa.Create(Curve);
        ecdsa.ImportECPrivateKey(key, out _);
        Key = key;
    }

    /// <summary>
    /// Derives a public key from the current private key
    /// </summary>
    /// <returns>A <see cref="PublicKey"/> object containing the key</returns>
    public PublicKey DerivePublicKey() {
        return new(ecdsa.ExportSubjectPublicKeyInfo());
    }
    
    /// <summary>
    /// Derives a public key from the current private key
    /// </summary>
    /// <returns>The bytes of the public key in the X.509 SubjectPublicKeyInfo format.</returns>
    public byte[] DerivePublicKeyBytes() => ecdsa.ExportSubjectPublicKeyInfo();

    public void Dispose() {
        ecdsa.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Signs a string using this private key.
    /// </summary>
    /// <param name="s">The string to be signed. Must be encoded in UTF8</param>
    /// <returns>A base64 string containing the signature bytes</returns>
    public string Sign(string s) {
        var bytes = Encoding.UTF8.GetBytes(s);
        var signatureBytes = Sign(bytes);
        return Convert.ToBase64String(signatureBytes);
    }

    /// <summary>
    /// Signs an array of bytes using this private key.
    /// </summary>
    /// <param name="bytes">The array to be signed</param>
    /// <returns>An array containing the signature bytes</returns>
    public byte[] Sign(byte[] bytes) => ecdsa.SignData(bytes, HashAlgorithmName.SHA512);

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
    /// Verifies that a data and a signature match the private key.
    /// </summary>
    /// <param name="data">An array of bytes with the content</param>
    /// <param name="signature">An array of bytes with the signature</param>
    /// <returns>A <see cref="bool"/> representing if the verification was successful or not.</returns>
    public bool Verify(byte[] data, byte[] signature) {
        return ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA512);
    }

    // a partir daqui é putaria
    public byte[] DeriveSharedSecret(PublicKey pubKey) {
        using var ecdh = ECDiffieHellman.Create();
        ecdh.ImportECPrivateKey(Key, out _); // read private key

        ECDiffieHellmanPublicKey ECDHpublicKey = ecdh.PublicKey;
        var shared = ecdh.DeriveKeyMaterial(ECDHpublicKey); // derive shared secret
        return shared;
    }

    public static byte[] EncryptSymmetricBytes(byte[] sharedSecret, byte[] data, byte[]? iv = null) {
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 256;
        aes.Key = sharedSecret; 

        iv ??= new byte[32];
        byte[] cypher = aes.EncryptCbc(data, iv);
        return cypher;
    }

    public static byte[] DecryptSymmetricBytes(byte[] sharedSecret, byte[] data, byte[]? iv = null) {
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 256;
        aes.Key = sharedSecret;

        iv ??= new byte[32];
        byte[] cypher = aes.DecryptCbc(data, iv);
        return cypher;
    }

    public static bool operator ==(PrivateKey p1, PrivateKey p2) => p1.Key.SequenceEqual(p2.Key);
    public static bool operator !=(PrivateKey p1, PrivateKey p2) => !(p1 == p2);

    public override string ToString() => Convert.ToHexString(Key);

    public override bool Equals(object? obj) {
        return obj is not null && obj is PrivateKey key &&
               Key.SequenceEqual(key.Key);
    }

    public override int GetHashCode() => Key.GetHashCode();


}