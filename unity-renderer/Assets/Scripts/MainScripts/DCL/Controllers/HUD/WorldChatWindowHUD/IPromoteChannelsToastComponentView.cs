using System;

namespace DCL.Social.Chat
{
    public interface IPromoteChannelsToastComponentView
    {
        event Action OnClose;
        void Show();
        void Hide();
        void Dispose();
    }
}
