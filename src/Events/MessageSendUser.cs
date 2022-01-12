using System;
using Unichain.Core;

namespace Unichain.Events
{
    public class MessageSendUser : BaseBlockChainEvent
    {
        public Address SenderAddress { get; set; }
        public Address ReceiverAddress { get; set; }

        public string Message { get; set; }

        public MessageSendUser(User sender, Address receiver, string message) : base(EventType.MessageSend, sender)
        {
            SenderAddress = sender.Address;
            ReceiverAddress = receiver;
            Message = receiver.Encrypt(message);
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
