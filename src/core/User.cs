using System;
using System.Collections.Generic;
using RodrigoChain.Core;

namespace RodrigoChain.Core
{
    /// <summary>
    /// Represents a user doing actions in the BlockChain
    /// </summary>
    public class User{
        public Address Address {get;set;}
        private PrivateKey PrivateKey {get;set;}
        public User(){
            PrivateKey = new PrivateKey();
            Address = PrivateKey.GetAddress();
        }
        public User(bool isNetwork){
            if(isNetwork){
                Address = new Address(true);
            }else{
                PrivateKey = new PrivateKey();
                Address = PrivateKey.GetAddress();
            }
           
        }
        /// <summary>
        /// Signs the given string with the current Private Key
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string SignMessage(string message){
            return PrivateKey.Sign(message);
        }
        /// <summary>
        /// Verifies if the signature matches the given signature
        /// </summary>
        /// <param name="message"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public bool VerifySignature(string message, string signature){
            return Address.VerifySign(message,signature);
        }

        public override bool Equals(object obj)
        {
            return obj is User user &&
                   EqualityComparer<Address>.Default.Equals(Address, user.Address) &&
                   EqualityComparer<PrivateKey>.Default.Equals(PrivateKey, user.PrivateKey);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Address, PrivateKey);
        }

        public static bool operator == (User a, Address b)=>a.Address.Equals(b);
        public static bool operator != (User a, Address b)=>!a.Address.Equals(b);
        public static bool operator == (User a, User b)=>a.Equals(b);
        public static bool operator != (User a, User b)=>!a.Equals(b);
        public static bool operator == (Address a, User b)=>b.Address.Equals(a);
        public static bool operator != (Address a, User b)=>!b.Address.Equals(a);

        public static explicit operator Address(User u)=>u.Address;
    }
}