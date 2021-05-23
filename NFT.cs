using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodrigoCoin_v2
{
    public class NFT
    {
        public Guid TokenId { get; }
        public object Metadata { get; }
        public string Owner { get; set; }
        public Uri ImageUri { get; }

        public NFT(string owner, object metadata, Uri imageuri)
        {
            this.TokenId = new Guid();
            this.Owner = owner;
            this.Metadata = metadata;
            this.ImageUri = imageuri;
        }
    }
}
