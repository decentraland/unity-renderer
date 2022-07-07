using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PublicChatEntry : BaseComponentView, IComponentModelConfig
{
    [SerializeField] internal Button openChatButton;
    [SerializeField] internal TMP_Text nameLabel;
    [SerializeField] internal PublicChatEntryModel model;
    [SerializeField] internal UnreadNotificationBadge unreadNotifications;
    [SerializeField] internal string namePrefix = "#";
    
    private IChatController chatController;

    public PublicChatEntryModel Model => model;

    public event Action<PublicChatEntry> OnOpenChat;

    public static PublicChatEntry Create()
    {
        return Instantiate(Resources.Load<PublicChatEntry>("SocialBarV1/PublicChannelElement"));
    }

    public override void Awake()
    {
        base.Awake();
        openChatButton.onClick.AddListener(() => OnOpenChat?.Invoke(this));
    }

    public void Initialize(IChatController chatController)
    {
        this.chatController = chatController;
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (PublicChatEntryModel) newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        nameLabel.text = $"{namePrefix}{model.name}";
        unreadNotifications.Initialize(chatController, model.channelId);
    }

    [Serializable]
    public class PublicChatEntryModel : BaseComponentModel
    {
        public string channelId;
        public string name;
        public long lastMessageTimestamp;

        public PublicChatEntryModel(string channelId, string name, long lastMessageTimestamp)
        {
            this.channelId = channelId;
            this.name = name;
            this.lastMessageTimestamp = lastMessageTimestamp;
        }
    }
}