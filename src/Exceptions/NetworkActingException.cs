using System;

namespace Unichain.Exceptions
{
    public class NetworkActingException : Exception
    {
        public NetworkActingException() : base("You cannot act as the network!")
        {
        }

        public NetworkActingException(string message) : base("You cannot act as the network!\nDescription: " + message)
        {
        }
    }
}
