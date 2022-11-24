using System;

namespace DCL.Social.Friends
{
    public interface ISendFriendRequestHUDView
    {
        event Action<string> OnMessageBodyChanged;
        event Action OnSend;
        event Action OnCancel;

        void Close();
        void Show();
        void Dispose();
        void SetName(string name);
        void ShowPendingToSend();
        void ShowSendSuccess();
        void ShowSendFailed();
    }
}