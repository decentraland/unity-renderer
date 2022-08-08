using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class ChannelContextualMenu : BaseComponentView
    {
        [Flags]
        internal enum Options
        {
            Leave = 1 << 0
        }

        [SerializeField] internal Options options;
        [SerializeField] internal TMP_Text headerTiler;
        [SerializeField] internal Button leaveButton;
        [SerializeField] internal Button closeButton;

        public event Action OnLeave;

        public override void Awake()
        {
            base.Awake();
            
            leaveButton.onClick.AddListener(() =>
            {
                OnLeave?.Invoke();
                Hide();
            });
            
            closeButton.onClick.AddListener(() => Hide());
            
            RefreshControl();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            gameObject.SetActive(true);
        }

        public override void Hide(bool instant = false)
        {
            base.Hide(instant);
            gameObject.SetActive(false);
        }

        public override void RefreshControl()
        {
            leaveButton.gameObject.SetActive((options & Options.Leave) != 0);
        }

        public void SetHeaderTitle(string title)
        {
            headerTiler.text = title;
        }
    }
}