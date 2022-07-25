using System;

namespace DCL.Chat.HUD
{
    public interface ICreateChannelWindowView
    {
        event Action<string> OnChannelNameUpdated;
        event Action OnCreateSubmit;
        event Action OnClose;
        void Show();
        void Hide();
        void ShowError(string message);
    }
}