using System;

namespace DCL.Helpers
{
    public class PromiseException : Exception
    {
        public PromiseException(string message) : base(message) { }
    }
}
