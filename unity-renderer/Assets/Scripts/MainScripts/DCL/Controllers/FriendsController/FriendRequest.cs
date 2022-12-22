namespace DCL.Social.Friends
{
    public class FriendRequest
    {
        public string FriendRequestId { get; }
        public long Timestamp { get; }
        public string From { get; }
        public string To { get; }
        public string MessageBody { get; }
        public FriendRequestState State { get; set; }

        public FriendRequest(string friendRequestId, long timestamp, string from, string to, string messageBody,
            FriendRequestState state)
        {
            FriendRequestId = friendRequestId;
            Timestamp = timestamp;
            From = from;
            To = to;
            MessageBody = messageBody;
            State = state;
        }

        public bool IsSentTo(string userId) =>
            To == userId;

        public bool IsReceivedFrom(string userId) =>
            From == userId;

        public bool IsPending() =>
            State == FriendRequestState.Pending;

        public bool IsCompleted() =>
            State != FriendRequestState.Pending;
    }
}
