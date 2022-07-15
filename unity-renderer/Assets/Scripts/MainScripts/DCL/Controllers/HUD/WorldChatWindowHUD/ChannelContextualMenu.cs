using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCL.Chat.HUD
{
    public class ChannelContextualMenu : MonoBehaviour
    {
        private enum Options
        {
            Leave = 1 << 0
        }

        [SerializeField] private Options options; 
        [SerializeField] private Button leaveButton;

        private RectTransform rectTransform;

        public event Action OnLeave;

        private void Start()
        {
            rectTransform = (RectTransform) transform; 
            leaveButton.onClick.AddListener(() =>
            {
                OnLeave?.Invoke();
                Hide();
            });
            leaveButton.gameObject.SetActive((options & Options.Leave) != 0);
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        private void Update()
        {
            HideWhenClickedOutsideArea();
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