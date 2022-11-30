using DCL.Helpers;
using System;

namespace DCL.Social.Friends
{
    public interface ICancelFriendRequestHUDView
    {
        event Action OnCancel;
        event Action OnClose;
        event Action OnOpenProfile;

        void SetName(string userName);
        void SetProfilePicture(ILazyTextureObserver textureObserver);
        void Show();
        void Close();
        void Dispose();
        void ShowPendingToCancel();
        void ShowCancelFailed();
        void SetBodyMessage(string messageBody);
        void SetTimestamp(DateTime date);
    }
}
