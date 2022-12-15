namespace DCL.Social.Friends
{
    public enum FriendRequestErrorCodes
    {
        TooManyRequestsSent = 0,
        NotEnoughTimePassed = 1,
        BlockedUser = 2,
        NonExistingUser = 3,
        InvalidRequest = 4,
        Unknown = 5
    }
}