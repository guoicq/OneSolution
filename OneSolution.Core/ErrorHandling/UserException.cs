using System;

namespace OneSolution.Core
{
    public class UserException: Exception
    {
        public UserException(string message):base(message) { }
    }
}
