using System;
using TMPro;
using UnityEngine;

public class VoiceChatBarComponentView : BaseComponentView, IVoiceChatBarComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal VoiceChatButton voiceChatButton;
    [SerializeField] internal GameObject playerNameContainer;
    [SerializeField] internal TMP_Text playerNameText;
    [SerializeField] internal Animator playerTalingAnimator;
    [SerializeField] internal ButtonComponentView endCallButton;

    [Header("Configuration")]
    [SerializeField] internal VoiceChatBarComponentModel model;

    public event Action<bool> OnMuteVoiceChat;
    public event Action OnLeaveVoiceChat;

    public RectTransform Transform => (RectTransform)transform;

    public override void Awake()
    {
        base.Awake();

        endCallButton.onClick.AddListener(() =>
        {
            OnLeaveVoiceChat?.Invoke();
        });
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

        SetPlayerName(model.playerName);
    }

    public override void Show(bool instant = false) 
    { 
        gameObject.SetActive(true);
        SetPlayerName(model.playerName);
    }

    public override void Hide(bool instant = false)
    { 
        gameObject.SetActive(false);
        SetPlayerName(string.Empty);
    }

    public void SetPlayerName(string playerName)
    {
        model.playerName = playerName;

        if (playerName == null)
            return;

        playerNameText.text = playerName;
        playerNameContainer.SetActive(!string.IsNullOrEmpty(playerName));
        playerTalingAnimator.SetBool("Talking", !string.IsNullOrEmpty(playerName));
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
