using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodrigoChain
{
    public interface ITokenMetadata
    {
        /// <summary>
        /// The Name of the token
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The human readable description for the token
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// A image url for the token, can be hosted on IPFS
        /// </summary>
        public string ImageUrl { get; set; }   
    }
}
