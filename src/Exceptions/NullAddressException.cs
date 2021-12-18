using System;

namespace Unichain.Exceptions
{
    public class NullAddressException : Exception
    {
        public NullAddressException() : base("The address is null!")
        {
        }

        public NullAddressException(string message) : base("The address is null!\nDescription: " + message)
        {
        }
    }
}
