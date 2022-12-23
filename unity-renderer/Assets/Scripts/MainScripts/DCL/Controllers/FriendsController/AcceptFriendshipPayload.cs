namespace DCL.Social.Friends
{
    public record AcceptFriendshipPayload
    {
        public FriendRequestPayload FriendRequest { get; set; }
    }
}
