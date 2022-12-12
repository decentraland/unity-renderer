using System;

namespace DCL.Social.Friends
{
    public class FriendshipException : Exception
    {
        public FriendRequestErrorCodes ErrorCode { get; }

        public FriendshipException(FriendRequestErrorCodes errorCode)
        {
            ErrorCode = errorCode;
        }
    }
}