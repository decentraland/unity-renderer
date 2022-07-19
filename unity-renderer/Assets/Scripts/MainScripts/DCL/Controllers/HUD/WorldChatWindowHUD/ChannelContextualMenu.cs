using System;
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
        [SerializeField] internal Button leaveButton;

        private RectTransform rectTransform;

        public event Action OnLeave;

        public override void Awake()
        {
            base.Awake();
            
            rectTransform = (RectTransform) transform; 
            leaveButton.onClick.AddListener(() =>
            {
                OnLeave?.Invoke();
                Hide();
            });
            
            RefreshControl();
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        public override void Update()
        {
            base.Update();
            
            HideWhenClickedOutsideArea();
        }

        public override void RefreshControl()
        {
            leaveButton.gameObject.SetActive((options & Options.Leave) != 0);
        }

        private void HideWhenClickedOutsideArea()
        {
            if (Input.GetMouseButtonDown(0) &&
                !RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition))
            {
                Hide();
            }
        }
    }
}