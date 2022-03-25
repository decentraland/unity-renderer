using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DirectChatEntry : BaseComponentView
{
    [SerializeField] private Button openChatButton;
    [SerializeField] private DirectChatEntryModel model;
    [SerializeField] private TMP_Text userNameLabel;
    [SerializeField] private TMP_Text lastMessageLabel;
    [SerializeField] private ImageComponentView picture;
    [SerializeField] private UnreadNotificationBadge unreadNotifications;
    [SerializeField] private Button optionsButton;
    [SerializeField] private GameObject blockedContainer;
    [SerializeField] private GameObject onlineStatusContainer;
    [SerializeField] private GameObject offlineStatusContainer;
    [SerializeField] private Transform userContextMenuPositionReference;

    private UserContextMenu userContextMenu;
    private IChatController chatController;

    public DirectChatEntryModel Model => model;

    public event Action OnOpenChat;

    public override void Awake()
    {
        base.Awake();
        optionsButton.onClick.AddListener(() =>
        {
            Dock(userContextMenu);
            userContextMenu.Show(model.userId);
        });
        openChatButton.onClick.AddListener(() => OnOpenChat?.Invoke());
    }

    public void Initialize(IChatController chatController,
        UserContextMenu userContextMenu)
    {
        this.chatController = chatController;
        this.userContextMenu = userContextMenu;
        userContextMenu.OnBlock -= HandleUserBlocked;
        userContextMenu.OnBlock += HandleUserBlocked;
    }

    public void Set(DirectChatEntryModel model)
    {
        this.model = model;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        userNameLabel.text = model.userName;
        lastMessageLabel.text = model.lastMessage;
        picture.Configure(new ImageComponentModel {uri = model.pictureUrl});
        SetBlockStatus(model.isBlocked);
        SetPresence(model.presence);
        unreadNotifications.Initialize(chatController, model.userId);
    }
    
    private void HandleUserBlocked(string userId, bool blocked)
    {
        if (userId != model.userId) return;
        SetBlockStatus(blocked);
    }

    private void SetBlockStatus(bool isBlocked)
    {
        model.isBlocked = isBlocked;
        blockedContainer.SetActive(isBlocked);
    }

    private void SetPresence(PresenceStatus presence)
    {
        model.presence = presence;
        onlineStatusContainer.SetActive(model.presence == PresenceStatus.ONLINE && !model.isBlocked);
        offlineStatusContainer.SetActive(model.presence != PresenceStatus.ONLINE && !model.isBlocked);
    }

    private void Dock(UserContextMenu userContextMenu) =>
        userContextMenu.transform.position = userContextMenuPositionReference.position;

    [Serializable]
    public struct DirectChatEntryModel
    {
        public string userId;
        public string userName;
        public string lastMessage;
        public string pictureUrl;
        public bool isBlocked;
        public PresenceStatus presence;

        public DirectChatEntryModel(string userId, string userName, string lastMessage, string pictureUrl, bool isBlocked, PresenceStatus presence)
        {
            this.userId = userId;
            this.userName = userName;
            this.lastMessage = lastMessage;
            this.pictureUrl = pictureUrl;
            this.isBlocked = isBlocked;
            this.presence = presence;
        }
    }
}