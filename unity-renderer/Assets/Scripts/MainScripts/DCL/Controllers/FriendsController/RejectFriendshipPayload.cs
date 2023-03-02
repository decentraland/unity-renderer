namespace DCL.Social.Friends
{
    public record RejectFriendshipPayload
    {
        public FriendRequestPayload FriendRequestPayload { get; set; }
    }
}
