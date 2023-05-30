using System;

namespace DCLServices.QuestsService
{
    public class UserIdNotSetException : Exception
    {
        public UserIdNotSetException() : base("UserId not set") { }
    }
}
