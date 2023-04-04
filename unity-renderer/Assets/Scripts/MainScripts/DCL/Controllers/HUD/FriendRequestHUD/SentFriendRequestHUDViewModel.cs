using DCL.Helpers;

namespace DCL.Social.Friends
{
    public record SentFriendRequestHUDViewModel
    {
        public string Name;
        public LayoutState State;
        public ILazyTextureObserver RecipientProfilePictureObserver;
        public ILazyTextureObserver SenderProfilePictureObserver;
        public string BodyMessage;

        public enum LayoutState
        {
            Default,
            Pending,
            Failed
        }
    }
}
