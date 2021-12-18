using NBitcoin;

namespace Unichain.Core
{
    /// <summary>
    /// A class representing a user's private key for signing and authorizing actions
    /// </summary>
    public class PrivateKey{
        public string Key {get;set;}
        private NBitcoin.Key key;
        public PrivateKey(){
            key = new Key();
            Key = key.ToHex();
        }

        public NBitcoin.Key GetRawKey(){
            return key;
        }

        public Address GetAddress(){
            var x = new Address(key.PubKey.ToHex());
            return x;
        }
        
        public string Sign(string s){
            return key.SignMessage(s);
        }

    }
}