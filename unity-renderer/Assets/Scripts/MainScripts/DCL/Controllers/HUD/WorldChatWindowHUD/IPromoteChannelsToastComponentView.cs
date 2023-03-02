using System;

namespace DCL.Chat.HUD
{
    public interface IPromoteChannelsToastComponentView
    {
        event Action OnClose;
        void Show();
        void Hide();
        void Dispose();
    }
}