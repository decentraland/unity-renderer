using System;
using System.Collections.Generic;
using DCL.Helpers;
using DCL.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PrivateChatWindowHUDView : MonoBehaviour, IPrivateChatComponentView
{
    private const string VIEW_PATH = "PrivateChatWindow";

    public Button backButton;
    public Button minimizeButton;
    public Button closeButton;
    public JumpInButton jumpInButton;
    public ChatHUDView chatHudView;
    public TMP_Text windowTitleText;
    public RawImage profilePictureImage;

    private UserProfile profile;

    public event Action OnPressBack;

    public IChatHUDComponentView ChatHUD => chatHudView;
    public bool IsActive => gameObject.activeSelf;
    public RectTransform Transform => (RectTransform) transform;

    public string userId { get; internal set; }

    public event Action OnMinimize;
    public event Action OnClose;

    void Awake()
    {
        minimizeButton.onClick.AddListener(OnMinimizeButtonPressed);
        closeButton.onClick.AddListener(OnCloseButtonPressed);
        backButton.onClick.AddListener(() => { OnPressBack?.Invoke(); });
    }

    void OnEnable() { Utils.ForceUpdateLayout(transform as RectTransform); }

    public static PrivateChatWindowHUDView Create()
    {
        return Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<PrivateChatWindowHUDView>();
    }

    private void OnMinimizeButtonPressed() => OnMinimize?.Invoke();

    private void OnCloseButtonPressed() => OnClose?.Invoke();
    
    public void Setup(UserProfile profile, bool isOnline, bool isBlocked)
    {
        this.profile = profile;
        ConfigureTitle(this.profile.userName);
        ConfigureUserId(this.profile.userId);
        this.profile.snapshotObserver?.RemoveListener(ConfigureAvatarSnapshot);
        this.profile.snapshotObserver?.AddListener(ConfigureAvatarSnapshot);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        chatHudView.scrollRect.verticalNormalizedPosition = 0;
        AudioScriptableObjects.dialogOpen.Play(true);
    }

    public void Hide()
    {
        profile?.snapshotObserver?.RemoveListener(ConfigureAvatarSnapshot);
        gameObject.SetActive(false);
        AudioScriptableObjects.dialogClose.Play(true);
    }

    public void Dispose()
    {
        profile?.snapshotObserver?.RemoveListener(ConfigureAvatarSnapshot);
        Destroy(gameObject);
    }

    private void ConfigureTitle(string targetUserName) { windowTitleText.text = targetUserName; }

    private void ConfigureAvatarSnapshot(Texture2D texture) { profilePictureImage.texture = texture; }

    private void ConfigureUserId(string userId)
    {
        this.userId = userId;
        jumpInButton.Initialize(FriendsController.i, userId);
    }
}