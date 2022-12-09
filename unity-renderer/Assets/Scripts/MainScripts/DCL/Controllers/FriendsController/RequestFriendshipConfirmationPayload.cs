using System;

namespace DCL.Social.Friends
{
    [Serializable]
    public class RequestFriendshipConfirmationPayload
    {
        public string messageId;
        public FriendRequestPayload friendRequest;
    }
}