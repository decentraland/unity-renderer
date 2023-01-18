using DCL.Helpers;
using System;

namespace DCL.Social.Friends
{
    public interface ISentFriendRequestHUDView : ISortingOrderUpdatable, IDisposable
    {
        event Action OnCancel;
        event Action OnClose;
        event Action OnOpenProfile;

        void SetRecipientName(string userName);
        void SetRecipientProfilePicture(ILazyTextureObserver textureObserver);
        void SetSenderProfilePicture(ILazyTextureObserver textureObserver);
        void Show(bool instant = false);
        void Close();
        void ShowPendingToCancel();
        void SetBodyMessage(string messageBody);
        void SetTimestamp(DateTime date);
    }
}
