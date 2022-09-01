using System;
using TMPro;
using UnityEngine;

public class VoiceChatBarComponentView : BaseComponentView, IVoiceChatBarComponentView, IComponentModelConfig<VoiceChatBarComponentModel>
{
    [Header("Prefab References")]
    [SerializeField] internal VoiceChatButton voiceChatButton;
    [SerializeField] internal GameObject someoneTalkingContainer;
    [SerializeField] internal TMP_Text someoneTalkingText;
    [SerializeField] internal TMP_Text altText;
    [SerializeField] internal Animator someoneTalkingAnimator;
    [SerializeField] internal ButtonComponentView endCallButton;
    [SerializeField] internal GameObject joinedPanel;
    [SerializeField] internal ButtonComponentView startCallButton;

    [Header("Configuration")]
    [SerializeField] internal VoiceChatBarComponentModel model;

    public event Action<bool> OnJoinVoiceChat;

    public RectTransform Transform => (RectTransform)transform;

    public override void Awake()
    {
        base.Awake();

        endCallButton.onClick.AddListener(() => OnJoinVoiceChat?.Invoke(false));
        startCallButton.onClick.AddListener(() => OnJoinVoiceChat?.Invoke(true));
    }

    public void Configure(VoiceChatBarComponentModel newModel)
    {
        model = newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        if (model == null)
            return;

        SetTalkingMessage(model.isSomeoneTalking, model.message);
        SetAsJoined(model.isJoined);
    }

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

    public void SetAsJoined(bool isJoined)
    {
        model.isJoined = isJoined;

        if (joinedPanel != null)
            joinedPanel.SetActive(isJoined);

        if (startCallButton != null)
            startCallButton.gameObject.SetActive(!isJoined);
    }

    public override void Dispose()
    {
        base.Dispose();

        endCallButton.onClick.RemoveAllListeners();
    }

    internal static VoiceChatBarComponentView Create()
    {
        VoiceChatBarComponentView voiceChatBarComponentView = Instantiate(Resources.Load<GameObject>("SocialBarV1/VoiceChatBar")).GetComponent<VoiceChatBarComponentView>();
        voiceChatBarComponentView.name = "_VoiceChatBar";

        return voiceChatBarComponentView;
    }
}
