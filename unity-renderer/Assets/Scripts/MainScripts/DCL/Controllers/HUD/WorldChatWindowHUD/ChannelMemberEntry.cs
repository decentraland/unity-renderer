using TMPro;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChannelMemberEntry : BaseComponentView, IComponentModelConfig<ChannelMemberEntryModel>
    {
        [SerializeField] internal TMP_Text nameLabel;
        [SerializeField] internal ImageComponentView userThumbnail;
        [SerializeField] internal GameObject onlineMark;
        [SerializeField] internal GameObject offlineMark;

        [Header("Configuration")]
        [SerializeField] internal ChannelMemberEntryModel model;

        public ChannelMemberEntryModel Model => model;

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
        }

        internal void SetUserId(string userId)
        {
            model.userId = userId;
        }

        internal void SetUserName(string userName)
        {
            model.userName = userName;

            if (nameLabel == null)
                return;

            nameLabel.text = userName;
        }

        internal void SetUserThumbnail(string thumbnailUrl)
        {
            model.thumnailUrl = thumbnailUrl;

            if (userThumbnail == null)
                return;

            userThumbnail.SetImage(thumbnailUrl);
        }

        internal void SetUserOnlineStatus(bool isOnline)
        {
            model.isOnline = isOnline;

            if (onlineMark == null)
                return;

            if (onlineMark != null)
                onlineMark.SetActive(isOnline);

            if (offlineMark != null)
                offlineMark.SetActive(!isOnline);
        }
    }
}