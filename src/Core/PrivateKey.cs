using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Math;
using BC_ECPoint = Org.BouncyCastle.Math.EC.ECPoint;

namespace Unichain.Core;

/// <summary>
/// A class representing a user's private key for signing and authorizing actions. 
/// Must be disposed after use!
/// </summary>
public class PrivateKey : IDisposable
{   
    /// <summary>
    /// The curve used in this key
    /// </summary>
    public static ECCurve Curve { get; } = ECCurve.NamedCurves.nistP521;
    private readonly ECDsa ecdsa;
    
    /// <summary>
    /// An array containing the key bytes.
    /// </summary>
    public byte[] Key { get; init; }

    private static ECDomainParameters bc_curve = BC_GetDomainParameters();
    private readonly BigInteger bc_d;

    /// <summary>
    /// Generates a random PrivateKey using <see cref="ECDsa"/> and <see cref="ECCurve.NamedCurves.nistP521"/> curve.
    /// </summary>
    public PrivateKey()
    {
        ecdsa = ECDsa.Create(Curve);
        Key = ecdsa.ExportECPrivateKey();
        var parameters = ecdsa.ExportExplicitParameters(true);
        bc_d = new BigInteger(parameters.D);
    }
    
    /// <summary>
    /// Initializes a PrivateKey from an existing key
    /// </summary>
    /// <param name="key">The bytes of the key in the ECPrivateKey structure</param>
    public PrivateKey(byte[] key) {
        ecdsa = ECDsa.Create(Curve);
        ecdsa.ImportECPrivateKey(key, out _);
        Key = key;
        var parameters = ecdsa.ExportExplicitParameters(true);
        bc_d = new BigInteger(parameters.D);
    }

    /// <summary>
    /// Derives a public key from the current private key
    /// </summary>
    /// <returns>A <see cref="PublicKey"/> object containing the key</returns>
    public PublicKey DerivePublicKey() {
        return new(ecdsa.ExportSubjectPublicKeyInfo(), BC_GetPublicKey());
    }
    
    /// <summary>
    /// Derives a public key from the current private key
    /// </summary>
    /// <returns>The bytes of the public key in the X.509 SubjectPublicKeyInfo format.</returns>
    public (byte[] key, byte[] bc_key) DerivePublicKeyBytes() => 
        (ecdsa.ExportSubjectPublicKeyInfo(), BC_GetPublicKey());

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

    /// <summary>
    /// Encrypts an byte array using a shared key between two parties.
    /// </summary>
    /// <param name="key">The shared secret. 256bits long</param>
    /// <param name="data">The data that will be encrypted</param>
    /// <param name="iv">A initialization vector. If null, a empty one will be used.
    /// It's 16 bytes long</param>
    /// <returns>An array containing the cyphertext</returns>
    public static byte[] EncryptBytes(byte[] key, byte[] data, byte[]? iv = null) {
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Key = key;

        iv ??= new byte[16];
        byte[] cypher = aes.EncryptCbc(data, iv);
        return cypher;
    }

    /// <summary>
    /// Decrypts an byte array using a shared key between two parties.
    /// </summary>
    /// <param name="key">The shared secret. 256bit long</param>
    /// <param name="data">An array containing the cyphertext</param>
    /// <param name="iv">A initialization vector. If null, a empty one will be used.
    /// It's 16 bytes long</param>
    /// <returns>An array containing the decrypted bytes</returns>
    public static byte[] DecryptBytes(byte[] key, byte[] data, byte[]? iv = null) {
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.KeySize = 256;
        aes.Key = key;

        iv ??= new byte[16];
        byte[] cypher = aes.DecryptCbc(data, iv);
        return cypher;
    }

    /// <summary>
    /// Performs an ECDH key exchange with another public key. Returns the shared secret.
    /// </summary>
    /// <param name="otherPublicKey">The public key that will make the exchange</param>
    /// <returns>A shared secret, 256 bits long</returns>
    public byte[] KeyExchange(PublicKey otherPublicKey) {
        // Q1 * D2 = Q2 * D1 => shared secret
        var bobQ = bc_curve.Curve.DecodePoint(otherPublicKey.BC_Key);
        var aliceD = bc_d;

        byte[] AESKey = BC_GenerateAESKey(bobQ, aliceD);
        return AESKey;
    }

    private static ECDomainParameters BC_GetDomainParameters() {
        X9ECParameters x9EC = NistNamedCurves.GetByName("P-521");
        ECDomainParameters ecDomain = new(x9EC.Curve, x9EC.G, x9EC.N, x9EC.H, x9EC.GetSeed());
        return ecDomain;
    }
    
    private byte[] BC_GetPublicKey() {
        // aquele bob.Key do DecodePoint pode cagar ainda!! rever isso
        BC_ECPoint q = bc_curve.G.Multiply(bc_d);
        var pubParams = new ECPublicKeyParameters(q, bc_curve);
        return pubParams.Q.GetEncoded();
    }
    
    private static byte[] BC_GenerateAESKey(BC_ECPoint publicKey, BigInteger privateKey) {
        var q = publicKey;
        var d = privateKey;
        var shared = q.Multiply(d);
        var sharedBytes = shared.GetEncoded();
        
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(sharedBytes);
        return hash;
    }

    public static bool operator ==(PrivateKey p1, PrivateKey p2) => p1.Key.SequenceEqual(p2.Key);
    public static bool operator !=(PrivateKey p1, PrivateKey p2) => !(p1 == p2);

    /// <summary>
    /// Gets the hex representation of the <see cref="Key"/>.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Convert.ToHexString(Key);


    /// <summary>
    /// Checks if two <see cref="PrivateKey"/> are equals
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object? obj) {
        return obj is not null && obj is PrivateKey key &&
               Key.SequenceEqual(key.Key);
    }

    /// <summary>
    /// Gets the hash code of the <see cref="Key"/> object.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => Key.GetHashCode();
}