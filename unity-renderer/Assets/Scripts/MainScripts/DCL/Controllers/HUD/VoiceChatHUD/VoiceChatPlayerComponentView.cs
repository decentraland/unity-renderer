using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VoiceChatPlayerComponentView : BaseComponentView, IVoiceChatPlayerComponentView, IComponentModelConfig
{
    [Header("Prefab References")]
    [SerializeField] internal ImageComponentView avatarPreview;
    [SerializeField] internal Sprite defaultUserSprite;
    [SerializeField] internal TextMeshProUGUI userName;
    [SerializeField] internal ButtonComponentView muteButton;
    [SerializeField] internal Image muteButtonImage;
    [SerializeField] internal ButtonComponentView unmuteButton;
    [SerializeField] internal GameObject blockedGO;
    [SerializeField] internal GameObject friendLabel;
    [SerializeField] internal GameObject backgroundHover;
    [SerializeField] internal Color talkingColor;
    [SerializeField] internal Color nonTalkingColor;

    [Header("Configuration")]
    [SerializeField] internal VoiceChatPlayerComponentModel model;

    public event Action<string, bool> OnMuteUser;

    public override void Awake()
    {
        base.Awake();

        muteButton.onClick.AddListener(() => SetAsMuted(true));
        unmuteButton.onClick.AddListener(() => SetAsMuted(false));
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (VoiceChatPlayerComponentModel)newModel;
        RefreshControl();
    }

    public override void RefreshControl() 
    {
        if (model == null)
            return;

        SetUserImage(model.userImageUrl);
        SetUserName(model.userName);
        SetAsMuted(model.isMuted);
        SetAsTalking(model.isTalking);
        SetAsBlocked(model.isBlocked);
        SetAsFriend(model.isFriend);
        SetBackgroundHover(model.isBackgroundHover);
    }

    public void SetUserId(string userId) { model.userId = userId; }

    public void SetUserImage(string url)
    {
        model.userImageUrl = url;

        if (avatarPreview == null)
            return;

        if (!string.IsNullOrEmpty(url))
            avatarPreview.SetImage(url);
        else
            avatarPreview.SetImage(defaultUserSprite);
    }

    public void SetUserName(string userName)
    {
        model.userName = userName;

        if (userName == null)
            return;

        this.userName.text = userName;
    }

    public void SetAsMuted(bool isMuted)
    {
        model.isMuted = isMuted;

        if (muteButton != null)
            muteButton.gameObject.SetActive(!isMuted);

        if (unmuteButton != null)
            unmuteButton.gameObject.SetActive(isMuted);

        OnMuteUser?.Invoke(model.userId, isMuted);
    }

    public void SetAsTalking(bool isTalking)
    {
        model.isTalking = isTalking;

        if (muteButtonImage == null)
            return;

        muteButtonImage.color = isTalking ? talkingColor : nonTalkingColor;
    }

    public void SetAsBlocked(bool isBlocked)
    {
        model.isBlocked = isBlocked;

        if (blockedGO == null)
            return;

        blockedGO.SetActive(isBlocked);
    }

    public void SetAsFriend(bool isFriend)
    {
        model.isFriend = isFriend;

        if (friendLabel == null)
            return;

        friendLabel.SetActive(isFriend);
    }

    public void SetBackgroundHover(bool isHover)
    {
        model.isBackgroundHover = isHover;

        if (backgroundHover == null)
            return;

        backgroundHover.SetActive(isHover);
    }

    public override void Dispose()
    {
        base.Dispose();

        muteButton.onClick.RemoveAllListeners();
        unmuteButton.onClick.RemoveAllListeners();
    }

    public override void OnFocus()
    {
        base.OnFocus();

        SetBackgroundHover(true);
    }

    public override void OnLoseFocus()
    {
        base.OnLoseFocus();

        SetBackgroundHover(false);
    }
}
