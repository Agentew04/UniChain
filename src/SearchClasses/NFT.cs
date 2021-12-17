using System;
using System.Collections.Generic;
using RodrigoChain.Core;
using RodrigoChain.Events;

namespace RodrigoChain{
    public class NFT{
        /// <summary>
        /// The unique Id for this Token
        /// </summary>
        public Guid NFTId { get; }

        /// <summary>
        /// The Address of the Owner of this Token
        /// </summary>
        public Address Owner { get; set; }

        /// <summary>
        /// The custom metadata for this Token
        /// </summary>
        public NFTMetadata NFTMetadata {get;set;}
        
        public NFT(NFTMint mint, IList<NFTTransfer> transfers){
            //get metadata from mint
            this.NFTMetadata = mint.NFTMetadata;
            this.NFTId = mint.NFTId;

            //check if transfers exist
            if(transfers.Count == 0 || transfers == null){
                //get owner from mint
                this.Owner = mint.Owner;
            }else{
                //get owner from last transfer
                this.Owner = transfers[transfers.Count-1].ToAddress;
            }
        }
    }
}