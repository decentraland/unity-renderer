using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class VoiceChatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] InputAction_Hold voiceChatAction;
    [SerializeField] Animator buttonAnimator;

    private static readonly int recordingTrigger = Animator.StringToHash("Recording");
    private static readonly int idleTrigger = Animator.StringToHash("Idle");

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
        if (isRecording == recording || !gameObject.activeInHierarchy)
            return;

        isRecording = recording;
        buttonAnimator.SetTrigger(isRecording ? recordingTrigger : idleTrigger);
    }
}