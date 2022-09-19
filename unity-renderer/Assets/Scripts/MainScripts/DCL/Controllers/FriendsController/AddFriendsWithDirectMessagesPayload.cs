using System;

namespace DCL.Friends.WebApi
{
    [Serializable]
    public class AddFriendsWithDirectMessagesPayload
    {
        public FriendWithDirectMessages[] currentFriendsWithDirectMessages;
        public int totalFriendsWithDirectMessages;
    }
}