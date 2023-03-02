using System;

namespace DCL.Chat.HUD
{
    public interface IChannelJoinErrorWindowView
    {
        event Action OnClose;
        event Action OnRetry;
        void Dispose();
        void Show(string channelName);
        void Hide();
    }
}