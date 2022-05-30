using System;
using UnityEngine;

public interface IVoiceChatBarComponentView
{
    event Action<bool> OnMuteVoiceChat;
    event Action OnLeaveVoiceChat;

    RectTransform Transform { get; }

    void Show(bool instant = false);
    void Hide(bool instant = false);
    void SetPlayerName(string userName);
    void SetAsMuted(bool isMuted);
}
