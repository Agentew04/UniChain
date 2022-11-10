#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using Unichain.Core;
using Unichain.Events;
using Xunit;

namespace Unichain.Tests
{
    public class BlockchainSearchTests
    {
        /// <summary>
        /// System under test
        /// </summary>
        private readonly Blockchain _sut;

        public BlockchainSearchTests()
        {
            _sut = new(2, 100);    
        }

        //public PoolCreate PreparePoolEnvironment()
        //{
        //    User user1 = new();

        //    PoolMetadata poolMetadata1 = new()
        //    {
        //        Options = new string[] { "option 1", "option 2", "option 3" },
        //        Description = "A simple pool",
        //        Name = "Default pool 1",
        //    };
        //    PoolCreate poolOpen1 = new(user1, poolMetadata1);
        //    poolOpen1.SignEvent(user1);

        //    _sut.AddEvent(poolOpen1);
        //    _sut.MinePendingTransactions(user1.Address);

        //    return poolOpen1;
        //}

        //public (Guid, string) PrepareVoteEnvironment()
        //{
        //    User user1 = new();
        //    User address1 = new();
        //    User address2 = new();
        //    User address3 = new();
        //    User address4 = new();


        //    PoolMetadata poolMetadata1 = new()
        //    {
        //        Options = new string[] { "option 1", "option 2", "option 3" },
        //        Description = "A simple pool",
        //        Name = "Default pool 1",
        //    };
        //    PoolCreate poolOpen1 = new(user1, poolMetadata1);
        //    poolOpen1.SignEvent(user1);

        //    PoolVote poolVote1 = new(address1, poolOpen1.PoolId, 0, _sut);
        //    PoolVote poolVote2 = new(address2, poolOpen1.PoolId, 1, _sut);
        //    PoolVote poolVote3 = new(address3, poolOpen1.PoolId, 1, _sut);
        //    PoolVote poolVote4 = new(address4, poolOpen1.PoolId, 2, _sut);
        //    poolVote1.SignEvent(address1);
        //    poolVote2.SignEvent(address2);
        //    poolVote3.SignEvent(address3);
        //    poolVote4.SignEvent(address4);

        //    _sut.AddEvent(poolOpen1);
        //    _sut.MinePendingTransactions(user1.Address);
        //    _sut.AddEvent(new List<ITransaction>() { poolVote1, poolVote2, poolVote3, poolVote4 });
        //    _sut.MinePendingTransactions(user1.Address);

        //    return (poolOpen1.PoolId, address2.Address);
        //}

        [Fact]
        public void Check_fees_and_balance_after_transaction()
        {
            User user1 = new();
            User user2 = new();
            User miner = new();

            // adds 100 to user1 balance
            _sut.MinePendingTransactions(user1.Address);
            
            ITransaction transaction = new CurrencyTransaction(user1, 5, user2.Address, 20);
            transaction.SignTransaction();
            
            _sut.AddEvent(transaction);
            _sut.MinePendingTransactions(miner.Address);

            Assert.Equal(_sut.Reward - 20 - 5, _sut.GetBalance(user1.Address));
            Assert.Equal(20, _sut.GetBalance(user2.Address));
            Assert.Equal(_sut.Reward, _sut.GetBalance(miner.Address));
        }

        [Fact]
        public void Message_of_currency_transaction_match() {
            User user1 = new();
            User user2 = new();
            User miner = new();

            // adds 100 to user1 balance
            _sut.MinePendingTransactions(user1.Address);

            var initialMessage = "Hello, World!";
            ITransaction transaction = new CurrencyTransaction(user1, 5, user2.Address, 20, initialMessage);
            transaction.SignTransaction();

            _sut.AddEvent(transaction);
            _sut.MinePendingTransactions(miner.Address);

            var foundMessage = _sut.Find<CurrencyTransaction>(x => x.Timestamp == transaction.Timestamp)
                .FirstOrDefault()?
                .Message;
            Assert.NotNull(foundMessage);
            Assert.Equal(initialMessage, foundMessage);
        }

        [Fact]
        public void Address_has_enough_balance()
        {
            User user1 = new(), user2 = new(), miner = new();

            double amount = 10;
            _sut.MinePendingTransactions(user1.Address); // user1 += 100
            ITransaction transaction = new CurrencyTransaction(user1, 0, user2.Address, amount); //user2 = amount,
                                                                                                 //user1-= amount
            transaction.SignTransaction();
            _sut.AddEvent(transaction);
            _sut.MinePendingTransactions(miner.Address);
            
            Assert.Equal(_sut.Reward - amount, _sut.GetBalance(user1.Address), 0.0001);
            Assert.Equal(amount, _sut.GetBalance(user2.Address), 0.0001);
        }

        //[Fact]
        //public void Get_all_pools()
        //{
        //    var pool = PreparePoolEnvironment();

        //    var foundpools = _sut.GetPools();
        //    IEnumerable<PoolCreate> correctpools = new PoolCreate[] { pool };

        //    Assert.Equal(correctpools, foundpools);
        //}

        //[Fact]
        //public void Get_pool_by_its_id()
        //{
        //    var pool = PreparePoolEnvironment();

        //    var foundpool = _sut.GetPoolById(pool.PoolId);
        //    Assert.Equal(pool, foundpool);
        //}

        //[Fact]
        //public void Get_sum_of_votes_in_pool()
        //{
        //    var (id, _) = PrepareVoteEnvironment();

        //    var votes = _sut.GetTotalVotes(id);
        //    Assert.Equal(4, votes);
        //}

        //[Fact]
        //public void Get_all_votes_in_pool()
        //{
        //    var (id, _) = PrepareVoteEnvironment();

        //    var votes = _sut.GetVotes(id);
        //    var expected = new List<int>() { 1, 2, 1 };
        //    Assert.Equal(expected, votes);
        //}

        //[Fact]
        //public void Get_amount_of_votes_in_option()
        //{
        //    var (id, _) = PrepareVoteEnvironment();

        //    var vote = _sut.GetVote(id, 1);
        //    var expected = 2;
        //    Assert.Equal(expected, vote);
        //}

        //[Fact]
        //public void Get_what_someone_voted()
        //{
        //    var (id, address) = PrepareVoteEnvironment();

        //    var voteindex = _sut.GetVoterOption(id, address);
        //    var expected = 1;
        //    Assert.Equal(expected, voteindex);
        //}
    }
}
#endif
