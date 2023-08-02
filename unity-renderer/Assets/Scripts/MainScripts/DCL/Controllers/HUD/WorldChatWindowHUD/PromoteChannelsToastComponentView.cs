using System;
using UnityEngine;

namespace DCL.Social.Chat
{
    public class PromoteChannelsToastComponentView : BaseComponentView, IPromoteChannelsToastComponentView
    {
        [SerializeField] internal ButtonComponentView closeButton;

        public event Action OnClose;

        public override void Awake()
        {
            base.Awake();

            closeButton.onClick.AddListener(() => OnClose?.Invoke());
        }

        public override void RefreshControl()
        {
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);
    }
}
