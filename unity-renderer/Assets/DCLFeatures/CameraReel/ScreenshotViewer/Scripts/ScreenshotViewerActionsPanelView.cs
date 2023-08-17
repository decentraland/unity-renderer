using System;
using UnityEngine;
using UnityEngine.UI;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public class ScreenshotViewerActionsPanelView : MonoBehaviour, IScreenshotViewerActionsPanelView
    {
        [SerializeField] private Button downloadButton;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Button linkButton;
        [SerializeField] private Button twitterButton;
        [SerializeField] private Button infoButton;

        public Image InfoButtonBackground => infoButton.image;

        public event Action DownloadClicked;
        public event Action DeleteClicked;
        public event Action LinkClicked;
        public event Action TwitterClicked;
        public event Action InfoClicked;

        private void Awake()
        {
            downloadButton.onClick.AddListener(() => DownloadClicked?.Invoke());
            deleteButton.onClick.AddListener(() => DeleteClicked?.Invoke());
            linkButton.onClick.AddListener(() => LinkClicked?.Invoke());
            twitterButton.onClick.AddListener(() => TwitterClicked?.Invoke());
            infoButton.onClick.AddListener(() => InfoClicked?.Invoke());
        }

        private void OnDestroy()
        {
            downloadButton.onClick.RemoveAllListeners();
            deleteButton.onClick.RemoveAllListeners();
            linkButton.onClick.RemoveAllListeners();
            twitterButton.onClick.RemoveAllListeners();
            infoButton.onClick.RemoveAllListeners();
        }
    }
}
