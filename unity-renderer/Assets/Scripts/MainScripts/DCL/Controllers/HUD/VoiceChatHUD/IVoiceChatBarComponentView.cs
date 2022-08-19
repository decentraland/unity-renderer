using System;
using UnityEngine;

public interface IVoiceChatBarComponentView
{
    event Action<bool> OnJoinVoiceChat;

    RectTransform Transform { get; }

    void SetTalkingMessage(bool isSomeoneTalking, string message);
    void PlayVoiceChatRecordingAnimation(bool recording);
    void SetVoiceChatEnabledByScene(bool enabled);
    void SetAsJoined(bool isJoined);
}
