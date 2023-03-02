using System;
using DCl.Social.Friends;

namespace DCL.Social.Friends
{
    [Serializable]
    public class AddFriendsWithDirectMessagesPayload
    {
        public FriendWithDirectMessages[] currentFriendsWithDirectMessages;
        public int totalFriendsWithDirectMessages;
    }
}