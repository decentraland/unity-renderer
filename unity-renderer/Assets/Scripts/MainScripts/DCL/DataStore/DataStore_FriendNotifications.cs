namespace DCL
{
    public class DataStore_FriendNotifications
    {
        public readonly BaseVariable<int> seenFriends = new BaseVariable<int>();
        public readonly BaseVariable<int> seenRequests = new BaseVariable<int>();
    }
}