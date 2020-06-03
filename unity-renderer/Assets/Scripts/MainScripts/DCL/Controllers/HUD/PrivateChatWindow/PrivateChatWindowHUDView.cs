using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DCL.Interface;

public class PrivateChatWindowHUDView : MonoBehaviour
{
    const string VIEW_PATH = "PrivateChatWindow";

    public Button backButton;
    public Button minimizeButton;
    public Button closeButton;
    public JumpInButton jumpInButton;
    public ChatHUDView chatHudView;
    public PrivateChatWindowHUDController controller;
    public TMP_Text windowTitleText;
    public Image profilePictureImage;

    public event System.Action OnPressBack;
    public event System.Action OnMinimize;
    public event System.Action OnClose;
    public UnityAction<ChatMessage> OnSendMessage;

    public string userId { get; internal set; }

    void Awake()
    {
        chatHudView.OnSendMessage += ChatHUDView_OnSendMessage;
    }

    void OnEnable()
    {
        DCL.Helpers.Utils.ForceUpdateLayout(transform as RectTransform);
    }

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
        if (string.IsNullOrEmpty(message.body)) return;

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

    public void ConfigureTitle(string targetUserName)
    {
        windowTitleText.text = targetUserName;
    }

    public void ConfigureProfilePicture(Sprite sprite)
    {
        profilePictureImage.sprite = sprite;
    }

    public void ConfigureUserId(string userId)
    {
        this.userId = userId;
        jumpInButton.Initialize(FriendsController.i, userId);
    }
}
