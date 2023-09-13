using System;
using TMPro;
using UIComponents.ContextMenu;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Social.Chat
{
    public class ChannelContextualMenu : ContextMenuComponentView
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
        [SerializeField] internal Button copyNameButton;
        [SerializeField] internal ShowHideAnimator nameCopiedToast;

        public event Action OnLeave;
        public event Action<string> OnNameCopied;

        public override void Awake()
        {
            base.Awake();

            leaveButton.onClick.AddListener(() =>
            {
                OnLeave?.Invoke();
                Hide();
            });

            closeButton.onClick.AddListener(() => Hide());
            copyNameButton.onClick.AddListener(() =>
            {
                OnNameCopied?.Invoke(headerTiler.text);

                nameCopiedToast.gameObject.SetActive(true);
                nameCopiedToast.ShowDelayHide(3);
            });

            RefreshControl();
        }

        public override void Show(bool instant = false)
        {
            base.Show(instant);
            gameObject.SetActive(true);
            ClampPositionToScreenBorders(transform.position);
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
