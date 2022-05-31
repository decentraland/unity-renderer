using System;
using TMPro;
using UnityEngine;

public class VoiceChatBarComponentView : BaseComponentView, IVoiceChatBarComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal VoiceChatButton voiceChatButton;
    [SerializeField] internal GameObject someoneTalkingContainer;
    [SerializeField] internal TMP_Text someoneTalkingText;
    [SerializeField] internal TMP_Text altText;
    [SerializeField] internal Animator someoneTalkingAnimator;
    [SerializeField] internal ButtonComponentView endCallButton;

    [Header("Configuration")]
    [SerializeField] internal VoiceChatBarComponentModel model;

    public event Action<bool> OnMuteVoiceChat;
    public event Action OnLeaveVoiceChat;

    public RectTransform Transform => (RectTransform)transform;

    public override void Awake()
    {
        base.Awake();

        endCallButton.onClick.AddListener(() => OnLeaveVoiceChat?.Invoke());
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (VoiceChatBarComponentModel)newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetTalkingMessage(model.isSomeoneTalking, model.message);
    }

    public override void Show(bool instant = false) { gameObject.SetActive(true); }

    public override void Hide(bool instant = false) { gameObject.SetActive(false); }

    public void SetTalkingMessage(bool isSomeoneTalking, string message)
    {
        model.message = message;

        if (someoneTalkingContainer != null)
            someoneTalkingContainer.SetActive(isSomeoneTalking);

        if (altText != null)
            altText.gameObject.SetActive(!isSomeoneTalking);

        if (isSomeoneTalking)
        {
            if (someoneTalkingText != null)
            {
                someoneTalkingText.text = message;
                someoneTalkingAnimator.SetBool("Talking", !string.IsNullOrEmpty(message));
            }
        }
        else
        {
            if (altText != null)
                altText.text = message;
        }
    }

    public void PlayVoiceChatRecordingAnimation(bool recording) { voiceChatButton.SetOnRecording(recording); }

    public void SetVoiceChatEnabledByScene(bool enabled) { voiceChatButton.SetEnabledByScene(enabled); }

    public override void Dispose()
    {
        base.Dispose();

        endCallButton.onClick.RemoveAllListeners();
    }

    internal static VoiceChatBarComponentView Create()
    {
        VoiceChatBarComponentView voiceChatBarComponentView = Instantiate(Resources.Load<GameObject>("VoiceChatBar")).GetComponent<VoiceChatBarComponentView>();
        voiceChatBarComponentView.name = "_VoiceChatBar";

        return voiceChatBarComponentView;
    }
}
