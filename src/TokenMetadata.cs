using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodrigoCoin_v2
{
    public class TokenMetadata
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
        /// <summary>
        /// The attributes for this token
        /// </summary>
        public Dictionary<object,object> Attributes { get; set; }
        /// <summary>
        /// Creates new metadata for creation of a token
        /// </summary>
        /// <param name="name">The name of the token</param>
        /// <param name="description">The description of the token</param>
        /// <param name="attributes">The attributes for this token</param>
        /// <param name="imageurl">The image url for the token</param>
        public TokenMetadata(string name, string description, Dictionary<object,object> attributes, string imageurl = "")
        {
            this.Name = name;
            this.Description = description;
            this.ImageUrl = imageurl;
            this.Attributes = attributes;
        }
        /// <summary>
        /// Returns JObject representing this metadata
        /// </summary>
        /// <returns></returns>
        public JObject ToJObject()
        {
            return JObject.FromObject(this);
        }
    }
}
