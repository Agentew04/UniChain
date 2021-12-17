using System.Collections.Generic;
using RodrigoChain.Events;

namespace RodrigoChain{
    public class NFT{

        public static NFT Parse(NFTMint mint, IEnumerable<NFTTransfer> transfers=null,NFTBurn burn=null){
            return new NFT();
        }
    }
}