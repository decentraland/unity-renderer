using DCL.Social.Chat;
using System;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class ChannelLimitReachedWindowComponentView : BaseComponentView, IChannelLimitReachedWindowView
    {
        [SerializeField] internal ButtonComponentView[] acceptButton;

        public event Action OnClose;

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        public override void Awake()
        {
            base.Awake();

            foreach (var button in acceptButton)
                button.onClick.AddListener(() => OnClose?.Invoke());
        }

        public override void RefreshControl()
        {
        }
    }
}
