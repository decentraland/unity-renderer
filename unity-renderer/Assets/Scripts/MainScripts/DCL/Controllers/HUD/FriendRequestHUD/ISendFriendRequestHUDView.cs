using DCL.Helpers;
using System;

namespace DCL.Social.Friends
{
    public interface ISendFriendRequestHUDView : IDisposable
    {
        event Action<string> OnMessageBodyChanged;
        event Action OnSend;
        event Action OnCancel;

        void Close();
        void Show(bool instant = false);
        void SetName(string name);
        void SetProfilePicture(ILazyTextureObserver textureObserver);
        void ShowPendingToSend();
        void ShowSendSuccess();
        void ClearInputField();
    }
}
