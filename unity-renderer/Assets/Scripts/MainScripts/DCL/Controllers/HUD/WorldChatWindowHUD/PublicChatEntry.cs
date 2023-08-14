using DCL.Social.Chat;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Chat
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
        [SerializeField] internal Button joinButton;
        [SerializeField] internal GameObject leaveButtonContainer;
        [SerializeField] internal GameObject openChatContainer;
        [SerializeField] internal GameObject joinedContainer;
        [SerializeField] internal Toggle muteNotificationsToggle;

        private IChatController chatController;
        private DataStore_Mentions mentionsDataStore;

        public PublicChatEntryModel Model => model;

        public event Action<PublicChatEntry> OnOpenChat;
        public event Action<PublicChatEntry> OnLeave;
        public event Action<PublicChatEntry> OnJoin;
        public event Action<PublicChatEntry> OnOpenOptions;

        public override void Awake()
        {
            base.Awake();
            openChatButton.onClick.AddListener(() => OnOpenChat?.Invoke(this));

            if (optionsButton)
                optionsButton.onClick.AddListener(() => OnOpenOptions?.Invoke(this));

            if (leaveButton)
                leaveButton.onClick.AddListener(() => OnLeave?.Invoke(this));

            if (joinButton)
                joinButton.onClick.AddListener(() => OnJoin?.Invoke(this));
        }

        public void Initialize(
            IChatController chatController,
            DataStore_Mentions mentionsDataStore)
        {
            this.chatController = chatController;
            this.mentionsDataStore = mentionsDataStore;
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
                unreadNotifications.Initialize(chatController, model.channelId, mentionsDataStore);
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
