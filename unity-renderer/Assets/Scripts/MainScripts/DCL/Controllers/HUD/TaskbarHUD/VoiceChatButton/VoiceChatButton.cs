using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class VoiceChatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] InputAction_Hold voiceChatAction;
    [SerializeField] Animator buttonAnimator;
    [SerializeField] private Animator disabledTooltipAnimator;

    private static readonly int talkingAnimation = Animator.StringToHash("Talking");
    private static readonly int disabledAnimation = Animator.StringToHash("Disabled");
    private static readonly int showDisabledTooltipAnimation = Animator.StringToHash("ShowDisabledTooltip");
    private static readonly int hideDisabledTooltipAnimation = Animator.StringToHash("HideDisabledTooltip");

    private bool isRecording = false;
    private bool isEnabledByScene = true;

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

    public void SetEnabledByScene(bool enabledByScene)
    {
        bool hasChangedToDisable = !enabledByScene && isEnabledByScene;
        if (hasChangedToDisable)
        {
            if (isRecording)
            {
                ShowDisabledTooltip();
            }
            voiceChatAction.OnStarted -= OnVoiceChatInput;
            voiceChatAction.OnStarted += OnVoiceChatInput;
        }
        else
        {
            voiceChatAction.OnStarted -= OnVoiceChatInput;
            disabledTooltipAnimator.SetTrigger(hideDisabledTooltipAnimation);
        }

        isEnabledByScene = enabledByScene;
        buttonAnimator.SetBool(disabledAnimation, !isEnabledByScene);
    }

    private void OnVoiceChatInput(DCLAction_Hold action)
    {
        if (!isEnabledByScene)
        {
            ShowDisabledTooltip();
        }
    }

    private void ShowDisabledTooltip()
    {
        if (disabledTooltipAnimator is null)
            return;

        disabledTooltipAnimator.SetTrigger(hideDisabledTooltipAnimation);
        disabledTooltipAnimator.SetTrigger(showDisabledTooltipAnimation);
    }
}