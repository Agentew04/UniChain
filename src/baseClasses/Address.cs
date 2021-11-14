using System;
using System.Text;
using NBitcoin;
using Org.BouncyCastle.Crypto.Digests;

namespace RodrigoChain
{
    /// <summary>
    /// A class for holding a RodrigoChain's Address
    /// </summary>
    public class Address{
        public string PublicKey {get;set;}
        public bool IsNetWork {get;set;}
        private NBitcoin.PubKey key;
        /// <summary>
        /// Creates a new random address
        /// </summary>
        public Address(){
            key = new NBitcoin.Key().PubKey;
            PublicKey = key.ToHex();
        }
        /// <summary>
        /// Creates a new address instance based on a address string
        /// </summary>
        /// <param name="publicKey"></param>
        public Address(string publicKey){
            if(publicKey=="network"){
                IsNetWork = true;
                return;
            }
            PublicKey = publicKey;
            key = new PubKey(publicKey);            
        }

        public Address(bool isNetwork){
            if(isNetwork){
                IsNetWork = true;
                return;
            }else{
                key = new NBitcoin.Key().PubKey;
                PublicKey = key.ToHex();
            return;
            }
        }
        public bool VerifySign(string originalmessage, string signature){
            return key.VerifyMessage(originalmessage,signature);
        }

        #region Defaults and operators

        /// <summary>
        /// Checks if the address is null, empty or whitespace
        /// </summary>
        /// <returns></returns>
        public bool IsNull(){
            if(IsNetWork){
                return false;
            }
            return string.IsNullOrWhiteSpace(PublicKey);
        }

        /// <summary>
        /// Checks if both Keys are equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj) => obj is Address && PublicKey.Equals(((Address)obj).PublicKey);

        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Returns the Hexadecimal Code for the PublicKey
        /// </summary>
        /// <returns></returns>
        public override string ToString() => PublicKey;

        /// <summary>
        /// Checks if both addresses are equal
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Address a, Address b)=> a.PublicKey.Equals(b.PublicKey);
        /// <summary>
        /// Checks if both addresses are different
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Address a, Address b)=> !(a==b);
        public static bool operator ==(Address a, string b)=> a.PublicKey.Equals(b);
        public static bool operator !=(Address a, string b)=> !(a==b);
        public static bool operator ==(string a, Address b)=> b==a;
        public static bool operator !=(string a, Address b)=> !(b==a);

        #endregion  
        
    }
}