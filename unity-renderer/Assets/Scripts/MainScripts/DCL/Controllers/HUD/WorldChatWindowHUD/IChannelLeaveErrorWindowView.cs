using System;

namespace DCL.Social.Chat
{
    public interface IChannelLeaveErrorWindowView
    {
        event Action OnClose;
        event Action OnRetry;
        void Dispose();
        void Show(string channelName);
        void Hide();
    }
}
