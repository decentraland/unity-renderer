using DCL.Helpers;

namespace DCL.Social.Friends
{
    public record SendFriendRequestHUDModel
    {
        public string Name;
        public LayoutState State;
        public ILazyTextureObserver ProfilePictureObserver;

        public enum LayoutState
        {
            Default,
            Pending,
            Success
        }
    }
}
