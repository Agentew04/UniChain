using System;

namespace Unichain.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException() : base("The token is invalid!")
        {
        }

        public InvalidTokenException(string message) : base("The token is invalid!\nDescription: " + message)
        {
        }
    }
}
