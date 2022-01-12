using Unichain.Core;
using Unichain.Events;
using Xunit;

namespace Unichain.Tests
{
    public class EventTests
    {
        private readonly Blockchain _sut;

        public EventTests()
        {
            _sut = new();
        }

        [Fact]
        public void Pool_vote_with_fee_is_valid()
        {
            User user = new();
            PoolMetadata pm = new()
            {
                Name = "Test pool",
                Description = "A test pool for testing purposes",
                Options = new string[] { "option1", "option2", "option3" },
                Fee = 15
            };
            PoolOpen po = new(user, pm);
            po.SignEvent(user);
            _sut.AddEvent(po);
            _sut.MinePendingTransactions(user.Address);

            PoolVote pv = new(user, po.PoolId, 0, _sut);
            pv.SignEvent(user);
            _sut.AddEvent(pv);
            _sut.MinePendingTransactions(user.Address);

            Assert.True(_sut.IsValid() && pv.IsValid(_sut));
        }

        [Fact]
        public void Transaction_is_valid()
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
