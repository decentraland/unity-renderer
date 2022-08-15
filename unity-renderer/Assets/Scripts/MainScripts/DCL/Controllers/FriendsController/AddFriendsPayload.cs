using System;

namespace DCL.Friends.WebApi
{
    [Serializable]
    public class AddFriendsPayload
    {
        public string[] friends;
        public int totalFriends;
    }
}