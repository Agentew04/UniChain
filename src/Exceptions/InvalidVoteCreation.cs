using System;

namespace Unichain.Exceptions
{
    public class InvalidVoteCreation : Exception
    {
        public InvalidVoteCreation() : base("The Vote Creation in invalid!")
        {
        }
        public InvalidVoteCreation(string message) : base("The vote creation is invalid!\nDescription: " + message)
        {
        }
    }
}
