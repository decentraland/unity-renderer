using DCL.Helpers;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public class ScreenshotViewerView : MonoBehaviour, IScreenshotViewerView
    {
        private const float SIDE_PANEL_ANIM_DURATION = 0.5f;

        [SerializeField] private ImageComponentView screenshotImage;
        [SerializeField] private RectTransform rootContainer;

        [Header("NAVIGATION BUTTONS")]
        [SerializeField] private Button closeView;
        [SerializeField] private Button prevScreenshotButton;
        [SerializeField] private Button nextScreenshotButton;

        private MetadataSidePanelAnimator metadataSidePanelAnimator;

        private bool metadataPanelIsOpen = true;

        [field: SerializeField] public ScreenshotViewerActionsPanelView ActionPanel { get; private set; }
        [field: SerializeField] public ScreenshotViewerInfoSidePanelView InfoSidePanel { get; private set; }

        public event Action CloseButtonClicked;
        public event Action PrevScreenshotClicked;
        public event Action NextScreenshotClicked;

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

        public void Dispose()
        {
            Utils.SafeDestroy(gameObject);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void SetScreenshotImage(string url)
        {
            screenshotImage.SetImage(url);
        }

        public void ToggleInfoSidePanel()
        {
            metadataSidePanelAnimator.ToggleSizeMode(toFullScreen: metadataPanelIsOpen, SIDE_PANEL_ANIM_DURATION);
            metadataPanelIsOpen = !metadataPanelIsOpen;
        }
    }
}
