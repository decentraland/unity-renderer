using DCL.Helpers;
using System;

namespace DCL.Social.Friends
{
    public interface IReceivedFriendRequestHUDView
    {
        event Action OnClose;
        event Action OnOpenProfile;
        event Action OnRejectFriendRequest;
        event Action OnConfirmFriendRequest;

        void SetBodyMessage(string messageBody);
        void SetTimestamp(DateTime timestamp);
        void SetRecipientName(string userName);
        void SetRecipientProfilePicture(string uri);
        void SetSenderProfilePicture(string uri);
        void Show();
        void Close();
        void ShowPendingToReject();
        void ShowRejectFailed();
        void ShowPendingToConfirm();
        void ShowAcceptFailed();
    }
}
