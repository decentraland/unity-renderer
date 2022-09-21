using System;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public interface IChannelLimitReachedWindowView
    {
        event Action OnClose;
        void Dispose();
        void Show();
        void Hide();
    }
}