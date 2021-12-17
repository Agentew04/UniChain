using System;

namespace RodrigoChain.Exceptions
{
    public class TokenNotFoundException : Exception
    {
        public TokenNotFoundException() :base("The token was not found on the Blockchain!")
        {
        }

        public TokenNotFoundException(string message) : base("The key provided was invalid!\nDescription: "+message)
        {
        }
    }
}
