using System;

namespace DCL.Friends.WebApi
{
    [Serializable]
    public class AddFriendsPayload
    {
        public string[] currentFriends;
        public int totalFriends;
    }
}