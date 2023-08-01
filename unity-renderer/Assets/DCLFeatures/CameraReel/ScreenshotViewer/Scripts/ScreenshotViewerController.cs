using Cysharp.Threading.Tasks;
using DCL;
using DCLFeatures.CameraReel.Section;
using DCLServices.CameraReelService;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Environment = DCL.Environment;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public class ScreenshotViewerController : IDisposable
    {
        private readonly ScreenshotViewerView view;
        private readonly CameraReelModel model;

        private CameraReelResponse currentScreenshot;

        public ScreenshotViewerController(ScreenshotViewerView view, CameraReelModel model)
        {
            this.view = view;
            this.model = model;
        }

        public void Initialize()
        {
            view.CloseButtonClicked += view.Hide;
            view.PrevScreenshotClicked += ShowPrevScreenshot;
            view.NextScreenshotClicked += ShowNextScreenshot;

            view.ActionPanel.DownloadClicked += DownloadScreenshot;
            view.ActionPanel.DeleteClicked += DeleteScreenshot;
            view.ActionPanel.LinkClicked += CopyScreenshotLink;
            view.ActionPanel.TwitterClicked += ShareOnTwitter;
            view.ActionPanel.InfoClicked += view.ToggleInfoSidePanel;

            view.InfoSidePanel.SidePanelButtonClicked += view.ToggleInfoSidePanel;
            view.InfoSidePanel.SceneButtonClicked += JumpInScene;
        }

        public void Dispose()
        {
            view.CloseButtonClicked -= view.Hide;
            view.PrevScreenshotClicked -= ShowPrevScreenshot;
            view.NextScreenshotClicked -= ShowNextScreenshot;

            view.ActionPanel.DownloadClicked -= DownloadScreenshot;
            view.ActionPanel.DeleteClicked -= DeleteScreenshot;
            view.ActionPanel.LinkClicked -= CopyScreenshotLink;
            view.ActionPanel.TwitterClicked -= ShareOnTwitter;
            view.ActionPanel.InfoClicked -= view.ToggleInfoSidePanel;

            view.InfoSidePanel.SidePanelButtonClicked -= view.ToggleInfoSidePanel;
            view.InfoSidePanel.SceneButtonClicked -= JumpInScene;
        }

        public void Show(CameraReelResponse reel)
        {
            if (reel == null) return;

            currentScreenshot = reel;

            SetScreenshotImage(reel);

            view.InfoSidePanel.SetSceneInfoText(reel.metadata.scene);
            view.InfoSidePanel.SetDateText(reel.metadata.GetLocalizedDateTime());
            view.InfoSidePanel.ShowVisiblePersons(reel.metadata.visiblePeople);

            view.Show();
        }

        private void ShowPrevScreenshot() =>
            Show(model.GetPreviousScreenshot(currentScreenshot));

        private void ShowNextScreenshot() =>
            Show(model.GetNextScreenshot(currentScreenshot));

        private async Task SetScreenshotImage(CameraReelResponse reel)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(reel.url);
            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.Log(request.error);
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                view.SetScreenshotImage(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
            }
        }

        private void DeleteScreenshot()
        {
            model.RemoveScreenshot(currentScreenshot);
            view.Hide();
        }

        private void DownloadScreenshot() =>
            Application.OpenURL(currentScreenshot.url);

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

        private void JumpInScene()
        {
            if (int.TryParse(currentScreenshot.metadata.scene.location.x, out int x) && int.TryParse(currentScreenshot.metadata.scene.location.y, out int y))
            {
                Environment.i.world.teleportController.JumpIn(x, y, currentScreenshot.metadata.realm, string.Empty);
                view.Hide();
                DataStore.i.exploreV2.isOpen.Set(false);
            }
        }
    }
}
