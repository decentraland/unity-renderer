using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class PrivateChatEntry : BaseComponentView, IComponentModelConfig<PrivateChatEntryModel>
    {
        [SerializeField] internal Button openChatButton;
        [SerializeField] internal PrivateChatEntryModel model;
        [SerializeField] internal TMP_Text userNameLabel;
        [SerializeField] internal TMP_Text lastMessageLabel;
        [SerializeField] internal ImageComponentView picture;
        [SerializeField] internal UnreadNotificationBadge unreadNotifications;
        [SerializeField] internal Button optionsButton;
        [SerializeField] internal GameObject blockedContainer;
        [SerializeField] internal GameObject onlineStatusContainer;
        [SerializeField] internal GameObject offlineStatusContainer;
        [SerializeField] internal RectTransform userContextMenuPositionReference;

        private UserContextMenu userContextMenu;
        private IChatController chatController;
        private DataStore_Mentions mentionsDataStore;

        public PrivateChatEntryModel Model => model;

        public event Action<PrivateChatEntry> OnOpenChat;

        public override void Awake()
        {
            base.Awake();
            optionsButton.onClick.AddListener(() =>
            {
                userContextMenu.Show(model.userId);
                Dock(userContextMenu);
            });
            openChatButton.onClick.AddListener(() => OnOpenChat?.Invoke(this));
            onFocused += isFocused =>
            {
                if (optionsButton == null) return;
                var isContextualMenuOpenWithThisUser = userContextMenu != null
                                                       && userContextMenu.isVisible
                                                       && userContextMenu.UserId == model.userId;
                optionsButton.gameObject.SetActive(isFocused || isContextualMenuOpenWithThisUser);
            };
        }

        public void Initialize(IChatController chatController,
            UserContextMenu userContextMenu,
            DataStore_Mentions mentionsDataStore)
        {
            this.chatController = chatController;
            this.userContextMenu = userContextMenu;
            this.mentionsDataStore = mentionsDataStore;
            userContextMenu.OnHide -= HandleContextMenuHidden;
            userContextMenu.OnHide += HandleContextMenuHidden;
            userContextMenu.OnBlock -= HandleUserBlocked;
            userContextMenu.OnBlock += HandleUserBlocked;
        }

        public void Configure(PrivateChatEntryModel newModel)
        {
            model = newModel;
            RefreshControl();
        }

        public override void RefreshControl()
        {
            userNameLabel.text = model.userName;
            lastMessageLabel.text = model.lastMessage;
            lastMessageLabel.gameObject.SetActive(!string.IsNullOrEmpty(model.lastMessage));
            SetBlockStatus(model.isBlocked);
            SetPresence(model.isOnline);
            unreadNotifications.Initialize(chatController, model.userId, mentionsDataStore);

            if (model.imageFetchingEnabled)
                EnableAvatarSnapshotFetching();
            else
                DisableAvatarSnapshotFetching();
        }

        private void HandleUserBlocked(string userId, bool blocked)
        {
            if (userId != model.userId) return;
            SetBlockStatus(blocked);
        }

        public void SetBlockStatus(bool isBlocked)
        {
            model.isBlocked = isBlocked;
            blockedContainer.SetActive(isBlocked);
        }

        public void SetPresence(bool isOnline)
        {
            model.isOnline = isOnline;
            onlineStatusContainer.SetActive(isOnline && !model.isBlocked);
            offlineStatusContainer.SetActive(!isOnline && !model.isBlocked);
        }

        private void Dock(UserContextMenu userContextMenu)
        {
            var menuTransform = (RectTransform) userContextMenu.transform;
            menuTransform.pivot = userContextMenuPositionReference.pivot;
            menuTransform.position = userContextMenuPositionReference.position;
        }

        public bool IsVisible(RectTransform container)
        {
            if (!gameObject.activeSelf) return false;
            return ((RectTransform) transform).CountCornersVisibleFrom(container) > 0;
        }

        public void EnableAvatarSnapshotFetching()
        {
            if (model.imageFetchingEnabled) return;
            picture.Configure(new ImageComponentModel {uri = model.pictureUrl});
            model.imageFetchingEnabled = true;
        }

        public void DisableAvatarSnapshotFetching()
        {
            if (!model.imageFetchingEnabled) return;
            picture.SetImage((string) null);
            model.imageFetchingEnabled = false;
        }

        private void HandleContextMenuHidden()
        {
            optionsButton.gameObject.SetActive(false);
        }
    }
}
