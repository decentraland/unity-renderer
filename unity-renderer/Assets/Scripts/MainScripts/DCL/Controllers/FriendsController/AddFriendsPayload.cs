using System;

namespace DCL.Social.Friends
{
    [Serializable]
    public class AddFriendsPayload
    {
        public string[] friends;
        public int totalFriends;
    }
}