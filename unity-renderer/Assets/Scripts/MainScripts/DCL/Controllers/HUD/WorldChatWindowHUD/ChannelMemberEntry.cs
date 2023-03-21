using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class ChannelMemberEntry : BaseComponentView, IComponentModelConfig<ChannelMemberEntryModel>
    {
        [SerializeField] internal TMP_Text nameLabel;
        [SerializeField] internal ImageComponentView userThumbnail;
        [SerializeField] internal GameObject onlineMark;
        [SerializeField] internal GameObject offlineMark;
        [SerializeField] internal GameObject optionsButtonContainer;
        [SerializeField] internal Button optionsButton;
        [SerializeField] internal RectTransform userContextMenuPositionReference;

        [Header("Configuration")]
        [SerializeField] internal ChannelMemberEntryModel model;

        private UserContextMenu userContextMenu;

        public ChannelMemberEntryModel Model => model;

        public override void Awake()
        {
            base.Awake();

            optionsButton.onClick.AddListener(() =>
            {
                userContextMenu.Show(model.userId);
                Dock(userContextMenu);
            });
        }

        public void Configure(ChannelMemberEntryModel newModel)
        {
            model = newModel;
            RefreshControl();
        }

        public override void RefreshControl()
        {
            SetUserId(model.userId);
            SetUserName(model.userName);
            SetUserThumbnail(model.thumnailUrl);
            SetUserOnlineStatus(model.isOnline);
            SetIsOptionsButtonHidden(model.isOptionsButtonHidden);
        }

        public void SetUserContextMenu(UserContextMenu userContextMenu)
        {
            this.userContextMenu = userContextMenu;
        }

        private void SetUserId(string userId)
        {
            model.userId = userId;
        }

        private void SetUserName(string userName)
        {
            model.userName = userName;

            if (nameLabel == null)
                return;

            nameLabel.text = userName;
        }

        private void SetUserThumbnail(string thumbnailUrl)
        {
            model.thumnailUrl = thumbnailUrl;

            if (userThumbnail == null)
                return;

            userThumbnail.SetImage(thumbnailUrl);
        }

        private void SetUserOnlineStatus(bool isOnline)
        {
            model.isOnline = isOnline;

            if (onlineMark == null)
                return;

            if (onlineMark != null)
                onlineMark.SetActive(isOnline);

            if (offlineMark != null)
                offlineMark.SetActive(!isOnline);
        }

        private void SetIsOptionsButtonHidden(bool isHidden)
        {
            model.isOptionsButtonHidden = isHidden;

            if (optionsButtonContainer == null)
                return;

            optionsButtonContainer.SetActive(!isHidden);
        }

        private void Dock(UserContextMenu userContextMenu)
        {
            var menuTransform = (RectTransform)userContextMenu.transform;
            menuTransform.pivot = userContextMenuPositionReference.pivot;
            menuTransform.position = userContextMenuPositionReference.position;
        }
    }
}