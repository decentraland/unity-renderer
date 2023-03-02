using System;
using TMPro;
using UnityEngine;

namespace DCL.Chat.HUD
{
    public class ChannelLeaveErrorWindowComponentView : BaseComponentView, IChannelLeaveErrorWindowView
    {
        [SerializeField] internal ButtonComponentView[] acceptButton;
        [SerializeField] internal ButtonComponentView retryButton;
        [SerializeField] internal TMP_Text titleLabel;
        
        public event Action OnClose;
        public event Action OnRetry;

        public void Show(string channelName)
        {
            gameObject.SetActive(true);
            titleLabel.SetText($"There was an error while trying to leave the channel #{channelName}. Please try again.");
        }

        public void Hide() => gameObject.SetActive(false);

        public static ChannelLeaveErrorWindowComponentView Create()
        {
            return Instantiate(
                Resources.Load<ChannelLeaveErrorWindowComponentView>("SocialBarV1/ChannelLeaveErrorModal"));
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