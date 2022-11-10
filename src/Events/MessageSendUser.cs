using System;
using System.Security.Cryptography;
using System.Text;
using Unichain.Core;

namespace Unichain.Events
{
    public class MessageSendUser : ITransaction
    {
        #region default variables

        public User Actor { get; set; }
        public double Fee { get; set; }
        public long Timestamp { get; set; } = DateTime.UtcNow.Ticks;
        public string TypeId { get; set; } = "transaction.msg.send";
        public string? Signature { get; set; }

        #endregion

        #region custom variables

        public string Message { get; set; }
        
        public bool IsEncrypted { get; set; }

        public byte[]? IV { get; set; }

        public PublicKey ToUser { get; set; }

        #endregion

        #region constructor

        public MessageSendUser(User actor,
            double fee,
            string message,
            PublicKey toUser) { 
            Actor = actor;
            Fee = fee;
            Message = message;
            ToUser = toUser;
            IsEncrypted = false;
        }

        #endregion

        #region methods

        public void EncryptMessage(PrivateKey? privateKey = null) {
            privateKey ??= Actor.PrivateKey;
            if (privateKey is null)
                throw new Exception("Neither a private key was provided nor the actor has one.");

            using var aes = Aes.Create();
            IV = aes.IV;
            IsEncrypted = true;
            var shared = privateKey.KeyExchange(ToUser);
            var plainText = Encoding.UTF8.GetBytes(Message);
            var cypherText = PrivateKey.EncryptBytes(shared, plainText, IV);
            Message = Convert.ToBase64String(cypherText);
        }

        #endregion
        
        #region Methods
        
        public string CalculateHash()
        {
            var bytes = Encoding.UTF8.GetBytes($"{Actor.Address}-{Message}-{IsEncrypted}-{Timestamp}-{ToUser.DeriveAddress()}");
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        public  bool IsValid(Blockchain blockchain)
        {
            double balance = blockchain.GetBalance(Actor.Address);
            if(balance < Fee)
                return false;
            if(string.IsNullOrWhiteSpace(Message)) 
                return false;

            if (Signature is null)
                return false;

            string hash = CalculateHash();
            return Actor.VerifySignature(hash, Signature);
        }

        public void SignTransaction(PrivateKey? key = null) {
            string hash = CalculateHash();
            if (key is null)
                Signature = Actor.SignString(hash);
            else
                Signature = key.Sign(hash);
        }

        #endregion
    }
}
