using System;

namespace RodrigoCoin_v2
{
    public class InvalidKeyException : Exception
    {
        public InvalidKeyException() : base("The key provided was invalid!")
        {
            
        }

        public InvalidKeyException(string message) : base("The key provided was invalid!\nDescription: "+message)
        {
        }
    }
    public class TokenNotFoundException : Exception
    {
        public TokenNotFoundException() :base("The token was not found on the Blockchain!")
        {
        }

        public TokenNotFoundException(string message) : base("The key provided was invalid!\nDescription: "+message)
        {
        }
    }
    public class InvalidTransactionException : Exception
    {
        public InvalidTransactionException() : base("The transaction is invalid!")
        {
        }

        public InvalidTransactionException(string message) : base("The transaction is invalid!\nDescription: " + message)
        {
        }
    }
    public class NetworkActingException : Exception
    {
        public NetworkActingException() : base("You cannot act as the network!")
        {
        }

        public NetworkActingException(string message) : base("You cannot act as the network!\nDescription: " + message)
        {
        }
    }
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException() : base("The token is invalid!")
        {
        }

        public InvalidTokenException(string message) : base("The token is invalid!\nDescription: " + message)
        {
        }
    }
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
