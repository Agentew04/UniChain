using Xunit;
using Unichain.Core;
using Unichain.Events;

namespace Unichain.Tests
{
    public class BlockchainTests
    {
        private readonly Blockchain _sut;

        public BlockchainTests()
        {
            _sut = new();
        }

        [Fact]
        public void IsValidShouldReturnTrue()
        {
            User user1 = new();
            Address address = new();

            Transaction transaction = new(user1, address, 10);
            transaction.SignEvent(user1);
            _sut.MinePendingTransactions(user1.Address);
            Assert.True(_sut.IsValid());
        }
    }
}
