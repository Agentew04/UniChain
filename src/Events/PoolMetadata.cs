using System;

namespace Unichain.Events
{
    public class PoolMetadata
    {

        /// <summary>
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
        /// An array of options that can be chosen
        /// </summary>
        /// <value></value>
        public string[] Options { get; set; } = Array.Empty<string>();

        /// <summary>
        /// The amount of currency paid to vote in 
        /// this pool. Default value is 0(no fee)
        /// </summary>
        /// <value></value>
        public double Fee { get; set; } = 0;

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
    }
}
