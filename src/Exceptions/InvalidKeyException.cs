using System;

namespace Unichain.Exceptions
{
    public class InvalidKeyException : Exception
    {
        public InvalidKeyException() : base("The key provided was invalid!")
        {

        }

        public InvalidKeyException(string message) : base("The key provided was invalid!\nDescription: " + message)
        {
        }
    }
}
