#if DEBUG
using Unichain.Core;
using Xunit;

namespace Unichain.Tests
{
    public class CoreTests
    {
        // TODO CREATE TESTS
        public CoreTests()
        {

        }

        [Fact]
        public void Create_address_from_string()
        {
            User user = new();
            
            Assert.True(user.Address == user.PublicKey.DeriveAddress());
        }

        [Fact]
        public void User_signing_and_verifying()
        {
            User user = new();
            string message = "Hello, world!";

            var signedmessage = user.SignMessage(message);
            var result = user.VerifySignature(message, signedmessage);

            Assert.True(result);
        }
    }
}
#endif
