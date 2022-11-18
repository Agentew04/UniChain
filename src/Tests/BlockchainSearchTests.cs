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
        

        [Fact]
        public void Get_pool_by_id() {
            User user1 = new(), user2 = new();

            PoolCreate pool = new(user1, 0.0, "testPool", new[] {
                "option1",
                "option2",
                "option3"
            });
            pool.SignTransaction();
            _sut.AddEvent(pool);
            _sut.MinePendingTransactions(user2.Address);
            
            PoolCreate? foundPool = _sut.GetPoolById(pool.PoolId);
            if(foundPool is null)
                Assert.Fail($"{nameof(foundPool)} should not be null!");
            
            Assert.Equal(pool.CalculateHash(), foundPool?.CalculateHash());
        }
        
        [Fact]
        public void Pool_votes_should_count()
        {
            User creator = new(), 
                voter1 = new(),
                voter2 = new(),
                voter3 = new();

            PoolCreate pool = new(creator, 0.0, "testPool", new[] {
                "option1",
                "option2",
                "option3"
            });
            pool.SignTransaction();
            
            PoolVote vote1 = new(voter1, 0.0, pool.PoolId, 0);
            PoolVote vote2 = new(voter2, 0.0, pool.PoolId, 1);
            PoolVote vote3 = new(voter3, 0.0, pool.PoolId, 2);
            vote1.SignTransaction();
            vote2.SignTransaction();
            vote3.SignTransaction();

            _sut.AddEvent(pool);
            _sut.MinePendingTransactions(creator.Address);
            
            _sut.AddEvent(new []{ vote1, vote2, vote3 } );
            _sut.MinePendingTransactions(creator.Address);
            
            List<int> votes = _sut.GetVotes(pool.PoolId);
            
            Assert.Equal(3, votes.Sum());
            Assert.True(votes.All(x => x == 1));
        }

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
