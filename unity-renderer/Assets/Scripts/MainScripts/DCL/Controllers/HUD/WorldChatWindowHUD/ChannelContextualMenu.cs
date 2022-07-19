using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
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
            if (!Input.GetMouseButtonDown(0)) return;
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
                
            if (raycastResults.All(result => result.gameObject != gameObject))
                Hide();
        }
    }
}