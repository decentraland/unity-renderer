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
    public PrivateChatWindowHUDController controller;
    public TMP_Text windowTitleText;
    public RawImage profilePictureImage;

    private readonly Dictionary<Action, UnityAction<string>> inputFieldListeners = new Dictionary<Action, UnityAction<string>>();
    private UserProfile profile;

    public event Action OnPressBack;
    public event Action OnInputFieldSelected
    {
        add
        {
            void Action(string s) => value.Invoke();
            inputFieldListeners[value] = Action;
            chatHudView.inputField.onSelect.AddListener(Action);
        }
        remove
        {
            if (!inputFieldListeners.ContainsKey(value)) return;
            chatHudView.inputField.onSelect.RemoveListener(inputFieldListeners[value]);
            inputFieldListeners.Remove(value);
        }
    }

    public IChatHUDComponentView ChatHUD => chatHudView;
    public bool IsActive => gameObject.activeSelf;
    public RectTransform Transform => (RectTransform) transform;

    public string userId { get; internal set; }

    public event Action OnMinimize;
    public event Action OnClose;
    public event Action<ChatMessage> OnSendMessage;

    void Awake() { chatHudView.OnSendMessage += ChatHUDView_OnSendMessage; }

    void OnEnable() { Utils.ForceUpdateLayout(transform as RectTransform); }

    public static PrivateChatWindowHUDView Create(PrivateChatWindowHUDController controller)
    {
        var view = Instantiate(Resources.Load<GameObject>(VIEW_PATH)).GetComponent<PrivateChatWindowHUDView>();
        view.Initialize(controller);
        return view;
    }

    private void Initialize(PrivateChatWindowHUDController controller)
    {
        this.controller = controller;
        this.minimizeButton.onClick.AddListener(OnMinimizeButtonPressed);
        this.closeButton.onClick.AddListener(OnCloseButtonPressed);
        this.backButton.onClick.AddListener(() => { OnPressBack?.Invoke(); });
    }

    public void ChatHUDView_OnSendMessage(ChatMessage message)
    {
        if (string.IsNullOrEmpty(message.body))
            return;

        message.messageType = ChatMessage.Type.PRIVATE;
        message.recipient = controller.conversationUserName;

        OnSendMessage?.Invoke(message);
    }

    public void OnMinimizeButtonPressed()
    {
        controller.SetVisibility(false);
        OnMinimize?.Invoke();
    }

    public void OnCloseButtonPressed()
    {
        controller.SetVisibility(false);
        OnClose?.Invoke();
    }
    
    public void Setup(UserProfile profile)
    {
        this.profile = profile;
        ConfigureTitle(this.profile.userName);
        ConfigureUserId(this.profile.userId);
        this.profile.snapshotObserver?.RemoveListener(ConfigureAvatarSnapshot);
        this.profile.snapshotObserver?.AddListener(ConfigureAvatarSnapshot);
    }

    public void CleanAllEntries() => chatHudView.CleanAllEntries();

    public void ResetInputField() => chatHudView.ResetInputField();

    public void FocusInputField() => chatHudView.FocusInputField();

    public void Show()
    {
        gameObject.SetActive(true);
        chatHudView.scrollRect.verticalNormalizedPosition = 0;
    }

    public void Hide()
    {
        profile?.snapshotObserver?.RemoveListener(ConfigureAvatarSnapshot);
        gameObject.SetActive(false);
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