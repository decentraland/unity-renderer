using System;
using DCL.Interface;
using SocialBar.UserThumbnail;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PrivateChatWindowComponentView : BaseComponentView, IPrivateChatComponentView
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private UserThumbnailComponentView userThumbnail;
    [SerializeField] private TMP_Text userNameLabel;
    [SerializeField] private PrivateChatHUDView chatView;
    [SerializeField] private Model model;

    public event Action OnPressBack;
    public event Action OnInputFieldSelected;
    public event Action<ChatMessage> OnSendMessage;
    public event Action OnMinimize;
    public event Action OnClose;

    public IChatHUDComponentView ChatHUD => chatView;
    public bool IsActive => gameObject.activeInHierarchy;
    public RectTransform Transform => (RectTransform) transform;

    public static PrivateChatWindowComponentView Create()
    {
        return Instantiate(Resources.Load<PrivateChatWindowComponentView>("SocialBarV1/PrivateChatHUD"));
    }

    public override void Awake()
    {
        base.Awake();
        backButton.onClick.AddListener(() => OnPressBack?.Invoke());
        backButton.onClick.AddListener(() => OnClose?.Invoke());
    }

    public override void RefreshControl()
    {
        userThumbnail.Configure(new UserThumbnailComponentModel
        {
            faceUrl = model.faceSnapshotUrl,
            isBlocked = model.isUserBlocked,
            isOnline = model.isUserOnline
        });
        userNameLabel.SetText(model.userName);
        chatView.ClearAllEntries();
        foreach (var model in model.entries)
            chatView.AddEntry(model.ToChatEntry(), true);
    }

    public void Setup(UserProfile profile, bool isOnline, bool isBlocked)
    {
        model = new Model
        {
            userId = profile.userId,
            faceSnapshotUrl = profile.face256SnapshotURL,
            userName = profile.userName,
            isUserOnline = isOnline,
            isUserBlocked = isBlocked
        };
        RefreshControl();
    }

    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    [Serializable]
    private struct Model
    {
        public string userId;
        public string userName;
        public string faceSnapshotUrl;
        public bool isUserBlocked;
        public bool isUserOnline;
        public ChatEntryModel[] entries;
    }

    [Serializable]
    private class ChatEntryModel
    {
        public string bodyText;
        public string senderId;
        public string senderName;
        public string recipientName;
        public string otherUserId;
        public ulong timestamp;
        public ChatEntry.Model.SubType type;

        public ChatEntry.Model ToChatEntry()
        {
            return new ChatEntry.Model
            {
                timestamp = timestamp,
                bodyText = bodyText,
                messageType = ChatMessage.Type.PRIVATE,
                otherUserId = otherUserId,
                recipientName = recipientName,
                senderId = senderId,
                senderName = senderName,
                subType = type
            };
        }
    }
}