using System;
using System.Collections.Generic;
using Unichain.Core;
using Unichain.Events;

namespace Unichain
{
    public class Pool
    {
        public string Owner { get; set; }
        public Guid PoolId { get; set; }
        // <summary>
        /// The name for this pool
        /// </summary>
        /// <value></value>
        public string Name { get; set; } = "";

        /// <summary>
        /// A description for this pool
        /// </summary>
        /// <value></value>
        public string Description { get; set; } = "";

        /// <summary>
        /// A dictionary with options and the amount of votes
        /// </summary>
        /// <value></value>
        public Dictionary<int, (string, int)> Options { get; set; } = new();

        /// <summary>
        /// The amount of currency paid to vote in 
        /// this pool. Default value is 0(no fee)
        /// </summary>
        /// <value></value>
        public int Fee { get; set; } = 0;

        /// <summary>
        /// Represents the minimum amount of currency 
        /// needed to be able to vote in this pool.
        /// Default value is 0(everybody can vote)
        /// </summary>
        /// <value></value>
        public int MinimumAmountToVote { get; set; } = 0;

        /// <summary>
        /// Max time to vote in this pool. Any votes after this
        /// time will not be valid. 
        /// Default value is 0(no time limit)
        /// </summary>
        /// <value></value>
        public long Deadline { get; set; } = 0L;

        public static Pool Parse(PoolOpen poolOpen, IEnumerable<PoolVote> poolVotes = null)
        {
            Pool pool = new()
            {
                Owner = poolOpen.Owner,
                PoolId = poolOpen.PoolId,
                Name = poolOpen.Metadata.Name,
                Description = poolOpen.Metadata.Description,
                Fee = poolOpen.Metadata.Fee,
                MinimumAmountToVote = poolOpen.Metadata.MinimumAmountToVote,
                Deadline = poolOpen.Metadata.Deadline
            };
            //a dictionary like <index,(description,votes)>
            Dictionary<int, (string, int)> options = new();
            //add option to dictionary
            foreach (string option in poolOpen.Metadata.Options)
            {
                options.Add(options.Count, (option, 0));
            }

            //if poolVotes is null, skip votes
            if (poolVotes == null)
            {
                pool.Options = options;
                return pool;
            }
            //add votes to dictionary
            foreach (PoolVote vote in poolVotes)
            {
                if (options.Count - 1 >= vote.VoteIndex)
                {
                    options[vote.VoteIndex] = (options[vote.VoteIndex].Item1, options[vote.VoteIndex].Item2 + 1);
                }
            }
            pool.Options = options;
            return pool;
        }
    }
}