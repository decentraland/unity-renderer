using System;

namespace DCL.Friends.WebApi
{
    [Serializable]
    public class AddFriendsPayload
    {
        public string[] friendsToAdd;
        public int totalFriends;
    }
}