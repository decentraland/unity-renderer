using System;

namespace DCL.Social.Friends
{
    [Serializable]
    public record ReceivedFriendRequestHUDModel
    {
        public LayoutState State;
        public string BodyMessage;
        public DateTime RequestDate;
        public string UserName;
        public string UserProfilePictureUri;
        public string OwnProfilePictureUri;

        public enum LayoutState
        {
            Default,
            Pending,
            ConfirmSuccess,
            RejectSuccess
        }
    }
}
