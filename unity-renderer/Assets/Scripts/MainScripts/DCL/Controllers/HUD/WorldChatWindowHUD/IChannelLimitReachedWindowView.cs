using System;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public interface IChannelLimitReachedWindowView
    {
        event Action OnClose;
        Transform Transform { get; }
        void Dispose();
        void Show();
        void Hide();
    }
}