using System;

namespace DCL.Social.Friends
{
    [Serializable]
    public class FriendshipUpdateStatusMessage
    {
        public string userId;
        public FriendshipAction action;
    }
}