using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class PublicChatEntry : BaseComponentView, IComponentModelConfig<PublicChatEntryModel>
    {
        [SerializeField] internal Button openChatButton;
        [SerializeField] internal TMP_Text nameLabel;
        [SerializeField] internal PublicChatEntryModel model;
        [SerializeField] internal UnreadNotificationBadge unreadNotifications;
        [SerializeField] internal string namePrefix = "#";
        [SerializeField] internal TMP_Text memberCountLabel;
        [SerializeField] internal Button optionsButton;
        [SerializeField] internal Button leaveButton;
        [SerializeField] internal GameObject leaveButtonContainer;
        [SerializeField] internal GameObject openChatContainer;
        [SerializeField] internal GameObject joinedContainer;
        [SerializeField] internal Toggle muteNotificationsToggle;
    
        private IChatController chatController;

        public PublicChatEntryModel Model => model;

        public event Action<PublicChatEntry> OnOpenChat;
        public event Action<PublicChatEntry> OnLeave;
        public event Action<PublicChatEntry> OnOpenOptions;

        public static PublicChatEntry Create()
        {
            return Instantiate(Resources.Load<PublicChatEntry>("SocialBarV1/PublicChannelElement"));
        }

        public override void Awake()
        {
            base.Awake();
            openChatButton.onClick.AddListener(() => OnOpenChat?.Invoke(this));
        
            if (optionsButton)
                optionsButton.onClick.AddListener(() => OnOpenOptions?.Invoke(this));
        
            if (leaveButton)
                leaveButton.onClick.AddListener(() => OnLeave?.Invoke(this));
        }

        public void Initialize(IChatController chatController)
        {
            this.chatController = chatController;
        }

        public void Configure(PublicChatEntryModel newModel)
        {
            model = newModel;
            RefreshControl();
        }

        public override void RefreshControl()
        {
            nameLabel.text = $"{namePrefix}{model.name}";
            if (unreadNotifications)
                unreadNotifications.Initialize(chatController, model.channelId);
            if (memberCountLabel)
                memberCountLabel.SetText($"{model.memberCount} members {(model.showOnlyOnlineMembers ? "online" : "joined")}");
            if (joinedContainer)
                joinedContainer.SetActive(model.isJoined);
            if (leaveButtonContainer)
                leaveButtonContainer.SetActive(model.isJoined);
            if (openChatContainer)
                openChatContainer.SetActive(!model.isJoined);
            if (muteNotificationsToggle)
                muteNotificationsToggle.SetIsOnWithoutNotify(model.muted);
        }

        public void Dock(ChannelContextualMenu contextualMenu)
        {
            contextualMenu.transform.position = optionsButton.transform.position;
        }
    }
}