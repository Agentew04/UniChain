using System;
using System.Collections.Generic;

namespace Unichain.Events
{
    [Obsolete("Use a Dictionary<string, object> instead")]
    public class NFTMetadata
    {
        /// <summary>
        /// The Name of the token
        /// </summary>
        public string Name { get; set; } = "";
        /// <summary>
        /// The human readable description for the token
        /// </summary>;
        public string Description { get; set; } = "";
        /// <summary>
        /// A image url for the token, can be hosted on IPFS
        /// </summary>
        public string ImageUrl { get; set; } = "";

        /// <summary>
        /// A dictionary of additional metadata for the token
        /// </summary>
        /// <value></value>
        public Dictionary<string, object> Metadata { get; set; } = new();

    }
}
