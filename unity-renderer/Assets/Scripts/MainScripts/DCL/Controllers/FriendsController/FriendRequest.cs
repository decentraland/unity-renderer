using System;
using System.Collections.Generic;

namespace DCL.Social.Friends
{
    public class FriendRequest
    {
        protected bool Equals(FriendRequest other) =>
            FriendRequestId == other.FriendRequestId && Timestamp == other.Timestamp && From == other.From && To == other.To && MessageBody == other.MessageBody;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FriendRequest)obj);
        }

        public override int GetHashCode() =>
            HashCode.Combine(FriendRequestId, Timestamp, From, To, MessageBody);

        public string FriendRequestId { get; }
        public long Timestamp { get; }
        public string From { get; }
        public string To { get; }
        public string MessageBody { get; }
        public bool HasBodyMessage => !string.IsNullOrEmpty(MessageBody);

        public FriendRequest(string friendRequestId, long timestamp, string from, string to, string messageBody)
        {
            FriendRequestId = friendRequestId;
            Timestamp = timestamp;
            From = from;
            To = to;
            MessageBody = messageBody;
        }

        public bool IsSentTo(string userId) =>
            To == userId;

        public bool IsReceivedFrom(string userId) =>
            From == userId;
    }
}
