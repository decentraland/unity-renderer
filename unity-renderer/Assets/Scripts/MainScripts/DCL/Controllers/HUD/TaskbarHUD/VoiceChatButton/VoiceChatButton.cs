using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class VoiceChatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] InputAction_Hold voiceChatAction;
    [SerializeField] Animator buttonAnimator;

    private static readonly int talkingAnimation = Animator.StringToHash("Talking");

    private bool isRecording = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        voiceChatAction.RaiseOnStarted();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        voiceChatAction.RaiseOnFinished();
    }

    public void SetOnRecording(bool recording)
    {
        isRecording = recording;

        if (!gameObject.activeInHierarchy)
            return;

        buttonAnimator.SetBool(talkingAnimation, recording);
    }
}