using System;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public interface ILeaveChannelConfirmationWindowComponentView
    {
        event Action OnCancelLeave;
        event Action<string> OnConfirmLeave;

        RectTransform Transform { get; }

        void SetChannel(string channelId);
        void Show(bool instant = false);
        void Hide(bool instant = false);
    }
}