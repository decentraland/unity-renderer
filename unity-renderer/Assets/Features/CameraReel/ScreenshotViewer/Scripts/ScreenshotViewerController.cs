using DCLServices.CameraReelService;
using System;
using UnityEngine;
using Environment = DCL.Environment;

namespace Features.CameraReel.ScreenshotViewer
{
    public class ScreenshotViewerController : IDisposable
    {
        private readonly ScreenshotViewerHUDView view;
        private readonly CameraReelModel model;

        private CameraReelResponse currentScreenshot => view.currentScreenshot;

        public ScreenshotViewerController(ScreenshotViewerHUDView view, CameraReelModel model)
        {
            this.view = view;
            this.model = model;
        }

        public void Initialize()
        {
            view.ActionPanel.DownloadClicked += DownloadScreenshot;
            view.ActionPanel.DeleteClicked += DeleteScreenshot;
            view.ActionPanel.LinkClicked += CopyScreenshotLink;
            view.ActionPanel.TwitterClicked += ShareOnTwitter;

            view.ActionPanel.InfoClicked += ToggleInfoPanel;
        }

        public void Dispose()
        {
            view.ActionPanel.DownloadClicked -= DownloadScreenshot;
            view.ActionPanel.DeleteClicked -= DeleteScreenshot;
            view.ActionPanel.LinkClicked -= CopyScreenshotLink;
            view.ActionPanel.TwitterClicked -= ShareOnTwitter;

            view.ActionPanel.InfoClicked -= ToggleInfoPanel;
        }

        private async void DeleteScreenshot()
        {
            ICameraReelNetworkService cameraReelNetworkService = Environment.i.serviceLocator.Get<ICameraReelNetworkService>();
            await cameraReelNetworkService.DeleteScreenshot(currentScreenshot.id);
        }

        private void DownloadScreenshot()
        {
            Application.OpenURL(currentScreenshot.url);
        }

        private void CopyScreenshotLink()
        {
            var url = $"https://dcl.gg/reels?image={currentScreenshot.id}";

            GUIUtility.systemCopyBuffer = url;
            Application.OpenURL(url);
        }

        private void ShareOnTwitter()
        {
            var description = "Check out what I'm doing in Decentraland right now and join me!";
            var url = $"https://dcl.gg/reels?image={currentScreenshot.id}";
            var twitterUrl = $"https://twitter.com/intent/tweet?text={description}&hashtags=DCLCamera&url={url}";

            GUIUtility.systemCopyBuffer = twitterUrl;
            Application.OpenURL(twitterUrl);
        }

        private void ToggleInfoPanel()
        {
            view.ToggleInfoPanel();
        }
    }
}
