﻿using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using DCLFeatures.CameraReel.Section;
using DCLServices.CameraReelService;
using System;
using System.Threading;
using UnityEngine;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public class ScreenshotViewerController : IDisposable
    {
        private const string DELETE_ERROR_MESSAGE = "There was an unexpected error when deleting the picture. Try again later.";

        private readonly ScreenshotViewerView view;
        private readonly CameraReelModel model;
        private readonly DataStore dataStore;
        private readonly ICameraReelStorageService storageService;
        private readonly IUserProfileBridge userProfileBridge;

        private CancellationTokenSource deleteScreenshotCancellationToken;
        private CancellationTokenSource pictureOwnerCancellationToken;
        private CameraReelResponse currentScreenshot;

        public ScreenshotViewerController(ScreenshotViewerView view, CameraReelModel model,
            DataStore dataStore,
            ICameraReelStorageService storageService,
            IUserProfileBridge userProfileBridge)
        {
            this.view = view;
            this.model = model;
            this.dataStore = dataStore;
            this.storageService = storageService;
            this.userProfileBridge = userProfileBridge;

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

            view.Dispose();
        }

        public void Show(CameraReelResponse reel)
        {
            if (reel == null) return;

            currentScreenshot = reel;

            view.SetScreenshotImage(reel.url);
            view.InfoSidePanel.SetSceneInfoText(reel.metadata.scene);
            view.InfoSidePanel.SetDateText(reel.metadata.GetLocalizedDateTime());
            view.InfoSidePanel.ShowVisiblePersons(reel.metadata.visiblePeople);
            pictureOwnerCancellationToken = pictureOwnerCancellationToken.SafeRestart();
            UpdatePictureOwnerInfo(reel, pictureOwnerCancellationToken.Token).Forget();

            view.Show();
        }

        private async UniTaskVoid UpdatePictureOwnerInfo(CameraReelResponse reel, CancellationToken cancellationToken)
        {
            UserProfile profile = userProfileBridge.Get(reel.metadata.userAddress)
                                                      ?? await userProfileBridge.RequestFullUserProfileAsync(reel.metadata.userAddress, cancellationToken);

            view.InfoSidePanel.SetPictureOwner(reel.metadata.userName, profile.face256SnapshotURL);
        }

        private void ShowPrevScreenshot() =>
            Show(model.GetPreviousScreenshot(currentScreenshot));

        private void ShowNextScreenshot() =>
            Show(model.GetNextScreenshot(currentScreenshot));

        private void DeleteScreenshot()
        {
            async UniTaskVoid DeleteScreenshotAsync(CameraReelResponse screenshot, CancellationToken cancellationToken)
            {
                try
                {
                    CameraReelStorageStatus storage = await storageService.DeleteScreenshot(screenshot.id, cancellationToken);
                    model.RemoveScreenshot(screenshot);
                    model.SetStorageStatus(storage.CurrentScreenshots, storage.MaxScreenshots);
                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    dataStore.notifications.DefaultErrorNotification.Set(DELETE_ERROR_MESSAGE, true);
                    Debug.LogException(e);
                }
                finally
                {
                    view.Hide();
                }
            }

            dataStore.notifications.GenericConfirmation.Set(new GenericConfirmationNotificationData(
                "Are you sure you want to delete this picture?",
                "This picture will be removed and you will no longer be able to access it.",
                "NO",
                "YES",
                () => {},
                () =>
                {
                    deleteScreenshotCancellationToken = deleteScreenshotCancellationToken.SafeRestart();
                    DeleteScreenshotAsync(currentScreenshot, deleteScreenshotCancellationToken.Token).Forget();
                }), true);
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
            if (!int.TryParse(currentScreenshot.metadata.scene.location.x, out int x)
                || !int.TryParse(currentScreenshot.metadata.scene.location.y, out int y))
                return;

            dataStore.HUDs.gotoPanelVisible.Set(true, true);
            dataStore.HUDs.gotoPanelCoordinates.Set((new ParcelCoordinates(x, y), currentScreenshot.metadata.realm), true);
            view.Hide();
            dataStore.exploreV2.isOpen.Set(false);
        }
    }
}
