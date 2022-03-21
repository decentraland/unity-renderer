using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DirectChatEntry : BaseComponentView
{
    [SerializeField] private DirectChatEntryModel model;
    [SerializeField] private TMP_Text userNameLabel;
    [SerializeField] private TMP_Text lastMessageLabel;
    [SerializeField] private ImageComponentView picture;
    [SerializeField] private UnreadNotificationBadge unreadNotifications;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Transform userContextMenuPositionReference;
    
    private UserContextMenu userContextMenu;
    private IChatController chatController;

    public DirectChatEntryModel Model => model;

    public override void Awake()
    {
        base.Awake();
        optionsButton.onClick.AddListener(() =>
        {
            if (userContextMenu != null)
             Dock(userContextMenu);
        });
    }

    public void Init(IChatController chatController,
        UserContextMenu userContextMenu)
    {
        this.chatController = chatController;
        this.userContextMenu = userContextMenu;
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
        unreadNotifications.Initialize(chatController, model.userId);
    }

    private void Dock(UserContextMenu userContextMenu) =>
        userContextMenu.transform.position = userContextMenuPositionReference.position;

    public struct DirectChatEntryModel
    {
        public string userId;
        public string userName;
        public string lastMessage;
        public string pictureUrl;

        public DirectChatEntryModel(string userId, string userName, string lastMessage, string pictureUrl)
        {
            this.userId = userId;
            this.userName = userName;
            this.lastMessage = lastMessage;
            this.pictureUrl = pictureUrl;
        }
    }
}