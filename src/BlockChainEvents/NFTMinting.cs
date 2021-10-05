﻿using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodrigoChain
{
    public class NFTMinting : BaseBlockChainEvent
    {
        #region Variables

        /// <summary>
        /// The unique Id for this Token
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// The Address of the Owner of this Token
        /// </summary>
        public Address Owner { get; set; }

        /// <summary>
        /// The custom metadata for this Token
        /// </summary>
        public ITokenMetadata Meta {get;set;}

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new token to be published on the blockchain
        /// </summary>
        /// <param name="owner">The first owner for the token</param>
        /// <param name="metadata">The metadata of this token</param>
        public NFTMinting(User user, ITokenMetadata metadata) : base(EventType.NFTMinting,user){
            this.ActionOwner=user;
            this.Timestamp = DateTime.UtcNow.ToFileTimeUtc();
            this.Meta=metadata;
            this.Owner=user.Address;
            this.Id = Guid.NewGuid();
        }

        #endregion

        #region Methods

        public override bool IsValid()
        {
            if( Signature == null) { return false; }
            if (Owner.IsNull() || Meta == null){ return false; }
            if (Meta.Name == null || Meta.Description == null){ return false; }
            if(Meta.Attributes == null || Meta.ImageUrl == null){ return false; }
            if (!VerifySignature()) { return false; }
            return true;
        }

        /// <summary>
        /// Returns a JObject equivalent to this <see cref="NFTMinting"/>, similar to <see cref="MoneyTransaction"/>
        /// </summary>
        /// <returns>A JObject that can be converted to string</returns>
        public JObject ToJObject()
        {
            return JObject.FromObject(this);
        }

        /// <summary>
        /// Returns a JSON serialized string
        /// </summary>
        /// <returns>A string in JSON format</returns>
        public override string ToString()
        {
            return this.ToJObject().ToString(Formatting.Indented);
        }

        public override string CalculateHash()
        {
            var sha = new Sha3Digest(512);
            byte[] input2 = Encoding.ASCII.GetBytes($"{this.Owner}-{this.Timestamp}-{this.Id}-{this.Meta}");

            sha.BlockUpdate(input2, 0, input2.Length);
            byte[] result = new byte[64];
            sha.DoFinal(result, 0);

            string hash = BitConverter.ToString(result);
            return hash.Replace("-", "").ToLowerInvariant();
        }

        public override void SignEvent(User user)
        {
            if (user.Address != this.Owner)
            {
                throw new Exception("Invalid key");
            }

            var hash = CalculateHash();
            var signature = user.SignMessage(hash);
            Signature = signature;
        }

        #endregion
    }
}
