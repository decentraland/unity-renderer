using System;
using TMPro;
using UnityEngine;

namespace DCL.Chat.HUD
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

        public static ChannelJoinErrorWindowComponentView Create()
        {
            return Instantiate(
                Resources.Load<ChannelJoinErrorWindowComponentView>("SocialBarV1/ChannelJoinErrorModal"));
        }

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