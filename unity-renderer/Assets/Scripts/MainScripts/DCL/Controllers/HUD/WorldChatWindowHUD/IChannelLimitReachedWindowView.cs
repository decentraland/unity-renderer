using System;
using UnityEngine;

namespace DCL.Social.Chat
{
    public interface IChannelLimitReachedWindowView
    {
        event Action OnClose;
        void Dispose();
        void Show();
        void Hide();
    }
}
