using DCL.Social.Chat;
using System;
using TMPro;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class ChannelJoinErrorWindowComponentView : BaseComponentView, IChannelJoinErrorWindowView
    {
        [SerializeField] internal ButtonComponentView[] acceptButton;
        [SerializeField] internal ButtonComponentView retryButton;
        [SerializeField] internal TMP_Text titleLabel;

        public event Action OnClose;
        public event Action OnRetry;

        public void Show(string channelName)
        {
            gameObject.SetActive(true);
            titleLabel.SetText($"There was an error while trying to join the channel #{channelName}. Please try again.");
        }

        public void Hide() => gameObject.SetActive(false);

        public override void Awake()
        {
            base.Awake();

            foreach (var button in acceptButton)
                button.onClick.AddListener(() => OnClose?.Invoke());

            retryButton.onClick.AddListener(() => OnRetry?.Invoke());
        }

        public override void RefreshControl()
        {
        }
    }
}
