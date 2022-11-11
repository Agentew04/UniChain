#if DEBUG
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Unichain.Core;
using Xunit;
using Xunit.Sdk;

namespace Unichain.Tests;

public class CoreTests {
    [Fact]
    public void Create_PrivateKey_from_bytes() {
        PrivateKey p1 = new();
        PrivateKey p2 = new(p1.Key);

        Assert.True( p1 == p2 );
    }


    [Fact]
    public void User_address_should_match_generated()
    {
        User user = new();
        string userAddress = user.Address;
        string generatedAddress = user.PublicKey.DeriveAddress();
        
        Assert.True( userAddress == generatedAddress );
    }

    [Fact]
    public void User_signature_match_message()
    {
        User user = new();
        string message = "Hello, world!";

        var signature = user.SignString(message);
        Assert.True(user.VerifySignature(message, signature));
    }

    [Fact]
    public void Public_from_private_match_bytes() {
        User user = new();

        if(user.PrivateKey is null)
            throw new Exception("Private key is null");

        (var pub, var bc_pub) = user.PrivateKey.DerivePublicKeyBytes();
        var publicKey = user.PrivateKey.DerivePublicKey();

        Assert.True(pub.SequenceEqual(publicKey.Key));
        Assert.True(bc_pub.SequenceEqual(publicKey.BC_Key));
    }

    [Fact]
    public void Modified_checksum_should_be_false() {
        User user = new();
        string brokenAddress = user.Address.Remove((user.Address.Length - 1) - 1, 2); // remove last 2
        brokenAddress += "00";
        Assert.False(PublicKey.IsAddressValid(brokenAddress));
    }

    [Fact]
    public void Odd_checksum_should_throw() {
        string address = "0x0000000000000000000000000000000000000000000"; // 3 verification chars

        var result = Assert.Throws<FormatException>(() => PublicKey.IsAddressValid(address));
        Assert.NotNull(result);
    }

    [Fact]
    public void Wrong_checksum_should_be_false() {
        string address = "0x000000000000000000000000000000000000000000000000"; // 8 verification chars
        Assert.False(PublicKey.IsAddressValid(address));
    }

    [Fact]
    public void Address_without_checksum_should_be_valid() {
        string address = "0x0000000000000000000000000000000000000000"; // 40 '0' chars after 0x
        Assert.True(PublicKey.IsAddressValid(address));
    }

    [Fact]
    public void Wrong_signature_should_be_false() {
        User alice = new();
        User bob = new();
        string message = "Hello, world!";
        string signature = alice.SignString(message);
        Assert.False(bob.VerifySignature(message, signature));
    }

    [Fact]
    public void Shared_secret_should_be_equals() {
        User alice = new();
        User bob = new();

        if(alice.PrivateKey! == bob.PrivateKey!)
            Assert.Fail("Private keys are equals");
        var aliceSecret = alice.PrivateKey?.KeyExchange(bob.PublicKey);
        var bobSecret = bob.PrivateKey?.KeyExchange(alice.PublicKey);

        Assert.True(aliceSecret?.SequenceEqual(bobSecret!));
    }

    [Fact]
    public void Encrypted_message_should_match() {
        User alice = new();
        User bob = new();

        byte[] plainText = Encoding.UTF8.GetBytes("Hello, world!");
        byte[] secret = alice.PrivateKey?.KeyExchange(bob.PublicKey)!;
        byte[] encrypted = PrivateKey.EncryptBytes(secret!, plainText);
        byte[] decrypted = PrivateKey.DecryptBytes(secret!, encrypted);

        Assert.True(plainText.SequenceEqual(decrypted));
    }
}
#endif
