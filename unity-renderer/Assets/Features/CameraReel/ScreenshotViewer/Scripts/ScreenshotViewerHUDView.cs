﻿using System;
using UnityEngine;
using UnityEngine.UI;

namespace Features.CameraReel.ScreenshotViewer
{
    public class ScreenshotViewerHUDView : MonoBehaviour
    {
        private const float SIDE_PANEL_ANIM_DURATION = 0.5f;

        [SerializeField] private Image screenshotImage;
        [SerializeField] public RectTransform rootContainer;

        [Header("NAVIGATION BUTTONS")]
        [SerializeField] private Button closeView;
        [SerializeField] private Button prevScreenshotButton;
        [SerializeField] private Button nextScreenshotButton;

        private MetadataSidePanelAnimator metadataSidePanelAnimator;

        [field: SerializeField] public ScreenshotViewerActionsPanelView ActionPanel { get; private set; }
        [field: SerializeField] public ScreenshotViewerInfoSidePanelView InfoSidePanel { get; private set; }

        public event Action CloseButtonClicked;
        public event Action PrevScreenshotClicked;
        public event Action NextScreenshotClicked;

        private bool metadataPanelIsOpen = true;

        private void Awake()
        {
            metadataSidePanelAnimator = new MetadataSidePanelAnimator(rootContainer, ActionPanel.InfoButtonBackground);
        }

        private void OnEnable()
        {
            closeView.onClick.AddListener(() => CloseButtonClicked?.Invoke());
            prevScreenshotButton.onClick.AddListener(() => PrevScreenshotClicked?.Invoke());
            nextScreenshotButton.onClick.AddListener(() => NextScreenshotClicked?.Invoke());
        }

        private void OnDisable()
        {
            closeView.onClick.RemoveAllListeners();
            prevScreenshotButton.onClick.RemoveAllListeners();
            nextScreenshotButton.onClick.RemoveAllListeners();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void ToggleInfoSidePanel()
        {
            metadataSidePanelAnimator.ToggleSizeMode(toFullScreen: metadataPanelIsOpen, SIDE_PANEL_ANIM_DURATION);
            metadataPanelIsOpen = !metadataPanelIsOpen;
        }

        public void SetScreenshotImage(Sprite sprite)
        {
            screenshotImage.sprite = sprite;
        }
    }
}
