using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.MyAccount
{
    public class BlockedUserEntry : BaseComponentView, IComponentModelConfig<BlockedUserEntryModel>
    {
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private Button unblockButton;

        [SerializeField] private ImageComponentView userThumbnail;

        [Header("Configuration")]
        [SerializeField] private BlockedUserEntryModel model;

        public BlockedUserEntryModel Model => model;

        public event Action<string> OnUnblockedClicked;

        public override void Awake()
        {
            base.Awake();

            unblockButton.onClick.AddListener(() =>
            {
                OnUnblockedClicked?.Invoke(model.userId);
            });
        }

        public void Configure(BlockedUserEntryModel newModel)
        {
            model = newModel;
            RefreshControl();
        }

        public void SetButtonAction(Action<string> action)
        {
            OnUnblockedClicked = action;
        }

        public override void RefreshControl()
        {
            SetUserId(model.userId);
            SetUserName(model.userName);
            SetUserThumbnail(model.thumbnailUrl);
        }

        private void SetUserId(string userId)
        {
            model.userId = userId;
        }

        private void SetUserName(string userName)
        {
            model.userName = userName;

            if (playerNameText == null)
                return;

            playerNameText.text = userName;
        }

        private void SetUserThumbnail(string thumbnailUrl)
        {
            model.thumbnailUrl = thumbnailUrl;

            if (userThumbnail == null)
                return;

            userThumbnail.SetImage(thumbnailUrl);
        }
    }
}
