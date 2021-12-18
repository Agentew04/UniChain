using System;

namespace Unichain.Exceptions
{
    public class InvalidTransactionException : Exception
    {
        public InvalidTransactionException() : base("The transaction is invalid!")
        {
        }

        public InvalidTransactionException(string message) : base("The transaction is invalid!\nDescription: " + message)
        {
        }
    }
}
