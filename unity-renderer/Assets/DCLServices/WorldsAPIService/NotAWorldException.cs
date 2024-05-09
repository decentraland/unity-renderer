using System;

namespace DCLServices.WorldsAPIService
{
    public class NotAWorldException : Exception
    {
        public NotAWorldException(string name) : base($"Couldn't find world with ID {name}") { }
    }
}
