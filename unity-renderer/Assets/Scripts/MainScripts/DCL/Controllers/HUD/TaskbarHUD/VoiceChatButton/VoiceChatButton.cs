using DCL;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VoiceChatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Animator buttonAnimator;
    [SerializeField] private Animator disabledTooltipAnimator;

    private static readonly int talkingAnimation = Animator.StringToHash("Talking");
    private static readonly int disabledAnimation = Animator.StringToHash("Disabled");
    private static readonly int showDisabledTooltipAnimation = Animator.StringToHash("ShowDisabledTooltip");
    private static readonly int hideDisabledTooltipAnimation = Animator.StringToHash("HideDisabledTooltip");

    private bool isRecording = false;
    private bool isEnabledByScene = true;

    public void OnPointerDown(PointerEventData eventData) { DataStore.i.voiceChat.isRecording.Set(new KeyValuePair<bool, bool>(true, false)); }

    public void OnPointerUp(PointerEventData eventData) { DataStore.i.voiceChat.isRecording.Set(new KeyValuePair<bool, bool>(false, false)); }

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
            DataStore.i.voiceChat.isRecording.OnChange -= OnVoiceChatInput;
            DataStore.i.voiceChat.isRecording.OnChange += OnVoiceChatInput;
        }
        else
        {
            DataStore.i.voiceChat.isRecording.OnChange -= OnVoiceChatInput;
            disabledTooltipAnimator.SetTrigger(hideDisabledTooltipAnimation);
        }

        isEnabledByScene = enabledByScene;
        buttonAnimator.SetBool(disabledAnimation, !isEnabledByScene);
    }

    private void OnVoiceChatInput(KeyValuePair<bool, bool> current, KeyValuePair<bool, bool> previous)
    {
        if (current.Key && !isEnabledByScene)
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