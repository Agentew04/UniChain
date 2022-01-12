using System;
using System.Collections.Generic;
using Unichain.Core;
using Unichain.Events;
using Xunit;

namespace Unichain.Tests
{
    public class BlockchainSearchTests
    {
        private readonly Blockchain _sut;

        public BlockchainSearchTests()
        {
            _sut = new();
        }

        public PoolOpen PreparePoolEnvironment()
        {
            User user1 = new();

            PoolMetadata poolMetadata1 = new()
            {
                Options = new string[] { "option 1", "option 2", "option 3" },
                Description = "A simple pool",
                Name = "Default pool 1",
            };
            PoolOpen poolOpen1 = new(user1, poolMetadata1);
            poolOpen1.SignEvent(user1);

            _sut.AddEvents(poolOpen1);
            _sut.MinePendingTransactions(user1.Address);

            return poolOpen1;
        }

        public (Guid, Address) PrepareVoteEnvironment()
        {
            User user1 = new();
            User address1 = new();
            User address2 = new();
            User address3 = new();
            User address4 = new();


            PoolMetadata poolMetadata1 = new()
            {
                Options = new string[] { "option 1", "option 2", "option 3" },
                Description = "A simple pool",
                Name = "Default pool 1",
            };
            PoolOpen poolOpen1 = new(user1, poolMetadata1);
            poolOpen1.SignEvent(user1);

            PoolVote poolVote1 = new(address1, poolOpen1.PoolId, 0, _sut);
            PoolVote poolVote2 = new(address2, poolOpen1.PoolId, 1, _sut);
            PoolVote poolVote3 = new(address3, poolOpen1.PoolId, 1, _sut);
            PoolVote poolVote4 = new(address4, poolOpen1.PoolId, 2, _sut);
            poolVote1.SignEvent(address1);
            poolVote2.SignEvent(address2);
            poolVote3.SignEvent(address3);
            poolVote4.SignEvent(address4);

            _sut.AddEvents(poolOpen1);
            _sut.MinePendingTransactions(user1.Address);
            _sut.AddEvents(poolVote1, poolVote2, poolVote3, poolVote4);
            _sut.MinePendingTransactions(user1.Address);

            return (poolOpen1.PoolId, address2.Address);
        }

        [Fact]
        public void Get_address_balance()
        {
            //create transactions
            User user1 = new();
            User user2 = new();

            Transaction transaction = new(user1, user2.Address, 20);
            transaction.SignEvent(user1);
            _sut.MinePendingTransactions(user1.Address);
            _sut.AddEvent(transaction);
            _sut.MinePendingTransactions(user1.Address);

            //check current balance
            Assert.Equal(20, _sut.GetBalance(user2.Address));
        }

        [Fact]
        public void Address_has_enough_balance()
        {
            //create transactions
            User user1 = new();
            User user2 = new();

            _sut.MinePendingTransactions(user1.Address);
            Transaction transaction = new(user1, user2.Address, 20);
            transaction.SignEvent(user1);
            _sut.AddEvent(transaction);
            _sut.MinePendingTransactions(user1.Address);

            var result = _sut.HasEnoughBalance(user2.Address, 15);

            Assert.True(result);
        }

        [Fact]
        public void Get_all_pools()
        {
            var pool = PreparePoolEnvironment();

            var foundpools = _sut.GetPools();
            IEnumerable<PoolOpen> correctpools = new PoolOpen[] { pool };

            Assert.Equal(correctpools, foundpools);
        }

        [Fact]
        public void Get_pool_by_its_id()
        {
            var pool = PreparePoolEnvironment();

            var foundpool = _sut.GetPoolById(pool.PoolId);
            Assert.Equal(pool, foundpool);
        }

        [Fact]
        public void Get_sum_of_votes_in_pool()
        {
            var (id, _) = PrepareVoteEnvironment();

            var votes = _sut.GetTotalVotes(id);
            Assert.Equal(4, votes);
        }

        [Fact]
        public void Get_all_votes_in_pool()
        {
            var (id, _) = PrepareVoteEnvironment();

            var votes = _sut.GetVotes(id);
            var expected = new List<int>() { 1, 2, 1 };
            Assert.Equal(expected, votes);
        }

        [Fact]
        public void Get_amount_of_votes_in_option()
        {
            var (id, address) = PrepareVoteEnvironment();

            var vote = _sut.GetVote(id, 1);
            var expected = 2;
            Assert.Equal(expected, vote);
        }

        [Fact]
        public void Get_what_someone_voted()
        {
            var (id, address) = PrepareVoteEnvironment();

            var voteindex = _sut.GetVoterOption(id, address);
            var expected = 1;
            Assert.Equal(expected, voteindex);
        }
    }
}
