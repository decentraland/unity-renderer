using System;
using UIComponents.Scripts.Components;
using static DCL.Social.Friends.ReceivedFriendRequestHUDModel;

namespace DCL.Social.Friends
{
    public interface IReceivedFriendRequestHUDView : IBaseComponentView<ReceivedFriendRequestHUDModel>, ISortingOrderUpdatable, IDisposable
    {
        event Action OnClose;
        event Action OnOpenProfile;
        event Action OnRejectFriendRequest;
        event Action OnConfirmFriendRequest;

        void SetBodyMessage(string messageBody);
        void SetTimestamp(DateTime timestamp);
        void SetSenderName(string userName);
        void SetSenderProfilePicture(string uri);
        void SetRecipientProfilePicture(string uri);
        void SetState(LayoutState state);
        void Close();
    }
}
