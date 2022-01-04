using Unichain.Core;
using Xunit;

namespace Unichain.Tests
{
    public class CoreTests
    {
        public CoreTests()
        {

        }

        [Fact]
        public void Create_address_from_string()
        {
            Address address = new();

            var pubkeystr = address.PublicKey;
            Address newaddr = new(pubkeystr);
            Assert.True(address==newaddr);
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

        [Fact]
        public void Network_address_is_flagged()
        {
            User user = new(true);
            Address address = new(true);
            Address address1 = new("network");

            Assert.True(user.Address.IsNetWork && address.IsNetWork && address1.IsNetWork);
        }
    }
}
