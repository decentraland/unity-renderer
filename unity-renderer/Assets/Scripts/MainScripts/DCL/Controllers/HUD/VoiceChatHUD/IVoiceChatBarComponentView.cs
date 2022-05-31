using System;
using UnityEngine;

public interface IVoiceChatBarComponentView
{
    event Action<bool> OnMuteVoiceChat;
    event Action OnLeaveVoiceChat;

    RectTransform Transform { get; }

    void Show(bool instant = false);
    void Hide(bool instant = false);
    void SetTalkingMessage(bool isSomeoneTalking, string message);
    void PlayVoiceChatRecordingAnimation(bool recording);
    void SetVoiceChatEnabledByScene(bool enabled);
}
