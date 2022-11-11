#if DEBUG
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
            _sut = new(2, 10);
        }

        //[Fact]
        //public void Pool_vote_with_fee_is_valid()
        //{
        //    User user = new();
        //    PoolMetadata pm = new()
        //    {
        //        Name = "Test pool",
        //        Description = "A test pool for testing purposes",
        //        Options = new string[] { "option1", "option2", "option3" },
        //        Fee = 15
        //    };
        //    PoolCreate po = new(user, pm);
        //    po.SignEvent(user);
        //    _sut.AddEvent(po);
        //    _sut.MinePendingTransactions(user.Address);

        //    PoolVote pv = new(user, po.PoolId, 0, _sut);
        //    pv.SignEvent(user);
        //    _sut.AddEvent(pv);
        //    _sut.MinePendingTransactions(user.Address);

        //    Assert.True(_sut.IsValid() && pv.IsValid(_sut));
        //}

        [Fact]
        public void Transaction_is_valid()
        {
            User user1 = new(), user2 = new();

            // get some coins(otherwise transaction is invalid)
            _sut.MinePendingTransactions(user1.Address);

            ITransaction transaction = new CurrencyTransaction(user1, 0, user2.Address, 5);
            transaction.SignTransaction();
            _sut.AddEvent(transaction);
            _sut.MinePendingTransactions(user1.Address);
            Assert.True(transaction.IsValid(_sut));
            Assert.True(_sut.IsValid());
            Assert.True(_sut.Chain[^1].HasValidTransactions(_sut));
        }
    }
}
#endif
