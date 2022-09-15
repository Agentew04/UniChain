using System;
using Unichain.Core;

namespace Unichain.Events
{
    public class MessageSendUser : BaseBlockChainEvent
    {
        public string SenderAddress { get; set; }
        public string ReceiverAddress { get; set; }

        public string Message { get; set; }

        public MessageSendUser(User sender, string receiver, string message) : base(EventType.MessageSendUser, sender)
        {
            SenderAddress = sender.Address;
            ReceiverAddress = receiver;
            //Message = receiver.Encrypt(message);
            Message = message; // encrypt later!
        }

        public override string CalculateHash()
        {
            throw new NotImplementedException();
        }

        public override bool IsValid(Blockchain blockchain)
        {
            throw new NotImplementedException();
        }

        public override void SignEvent(User user)
        {
            throw new NotImplementedException();
        }
    }
}
