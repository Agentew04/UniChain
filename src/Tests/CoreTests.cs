#if DEBUG
using System.Linq;
using System.Security.Cryptography;
using Unichain.Core;
using Xunit;

namespace Unichain.Tests;

public class CoreTests
{
    public CoreTests()
    {

    }

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

        var publicBytes = user.PrivateKey.DerivePublicKeyBytes();
        var publicKey = user.PrivateKey.DerivePublicKey().Key;

        Assert.True(publicBytes.SequenceEqual(publicKey));
    }
}
#endif
