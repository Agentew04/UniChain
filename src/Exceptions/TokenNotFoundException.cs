using System;

namespace Unichain.Exceptions
{
    public class NFTNotFoundException : Exception
    {
        public NFTNotFoundException() : base("The token was not found on the Blockchain!")
        {
        }

        public NFTNotFoundException(string message) : base("The key provided was invalid!\nDescription: " + message)
        {
        }
    }
}
