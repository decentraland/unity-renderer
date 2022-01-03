using System;

namespace DCL
{
    public class AvatarLoadFatalException : Exception
    {
        public AvatarLoadFatalException(string message) : base(message)
        {
        }
    }
}