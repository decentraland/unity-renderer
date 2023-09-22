using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCL.Tasks;
using DCLFeatures.CameraReel.Section;
using DCLServices.CameraReelService;
using DCLServices.EnvironmentProvider;
using System;
using System.Threading;
using UnityEngine;

namespace DCLFeatures.CameraReel.ScreenshotViewer
{
    public class ScreenshotViewerController : IDisposable
    {
        private const string SCREEN_SOURCE = "ReelPictureDetail";
        private const string DELETE_ERROR_MESSAGE = "There was an unexpected error when deleting the picture. Try again later.";

        private readonly IScreenshotViewerView view;
        private readonly CameraReelModel model;
        private readonly DataStore dataStore;
        private readonly ICameraReelStorageService storageService;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IClipboard clipboard;
        private readonly IBrowserBridge browserBridge;
        private readonly ICameraReelAnalyticsService analytics;
        private readonly IEnvironmentProviderService environmentProviderService;
        private readonly IScreenshotViewerActionsPanelView actionsPanelView;
        private readonly IScreenshotViewerInfoSidePanelView infoSidePanelView;

        private CancellationTokenSource deleteScreenshotCancellationToken;
        private CancellationTokenSource pictureOwnerCancellationToken;
        private CameraReelResponse currentScreenshot;

        public ScreenshotViewerController(IScreenshotViewerView view,
            CameraReelModel model,
            DataStore dataStore,
            ICameraReelStorageService storageService,
            IUserProfileBridge userProfileBridge,
            IClipboard clipboard,
            IBrowserBridge browserBridge,
            ICameraReelAnalyticsService analytics,
            IEnvironmentProviderService environmentProviderService,
            IScreenshotViewerActionsPanelView actionsPanelView,
            IScreenshotViewerInfoSidePanelView infoSidePanelView)
        {
            this.view = view;
            this.model = model;
            this.dataStore = dataStore;
            this.storageService = storageService;
            this.userProfileBridge = userProfileBridge;
            this.clipboard = clipboard;
            this.browserBridge = browserBridge;
            this.analytics = analytics;
            this.environmentProviderService = environmentProviderService;
            this.actionsPanelView = actionsPanelView;
            this.infoSidePanelView = infoSidePanelView;

            view.CloseButtonClicked += view.Hide;
            view.PrevScreenshotClicked += ShowPrevScreenshot;
            view.NextScreenshotClicked += ShowNextScreenshot;

            actionsPanelView.DownloadClicked += DownloadScreenshot;
            actionsPanelView.DeleteClicked += DeleteScreenshot;
            actionsPanelView.LinkClicked += CopyScreenshotLink;
            actionsPanelView.TwitterClicked += ShareOnTwitter;
            actionsPanelView.InfoClicked += view.ToggleInfoSidePanel;

            infoSidePanelView.SidePanelButtonClicked += view.ToggleInfoSidePanel;
            infoSidePanelView.SceneButtonClicked += JumpInScene;
            infoSidePanelView.OnOpenPictureOwnerProfile += OpenPictureOwnerProfile;
        }

        public void Dispose()
        {
            view.CloseButtonClicked -= view.Hide;
            view.PrevScreenshotClicked -= ShowPrevScreenshot;
            view.NextScreenshotClicked -= ShowNextScreenshot;

            actionsPanelView.DownloadClicked -= DownloadScreenshot;
            actionsPanelView.DeleteClicked -= DeleteScreenshot;
            actionsPanelView.LinkClicked -= CopyScreenshotLink;
            actionsPanelView.TwitterClicked -= ShareOnTwitter;
            actionsPanelView.InfoClicked -= view.ToggleInfoSidePanel;

            infoSidePanelView.SidePanelButtonClicked -= view.ToggleInfoSidePanel;
            infoSidePanelView.SceneButtonClicked -= JumpInScene;
            infoSidePanelView.OnOpenPictureOwnerProfile -= OpenPictureOwnerProfile;

            view.Dispose();
        }

        public void Show(CameraReelResponse reel)
        {
            if (reel == null) return;

            currentScreenshot = reel;

            view.SetScreenshotImage(reel.url);
            infoSidePanelView.SetSceneInfoText(reel.metadata.scene);
            infoSidePanelView.SetDateText(reel.metadata.GetLocalizedDateTime());
            infoSidePanelView.ShowVisiblePersons(reel.metadata.visiblePeople);
            pictureOwnerCancellationToken = pictureOwnerCancellationToken.SafeRestart();
            UpdatePictureOwnerInfo(reel, pictureOwnerCancellationToken.Token).Forget();

            view.Show();
        }

        private async UniTaskVoid UpdatePictureOwnerInfo(CameraReelResponse reel, CancellationToken cancellationToken)
        {
            UserProfile profile = userProfileBridge.Get(reel.metadata.userAddress)
                                                      ?? await userProfileBridge.RequestFullUserProfileAsync(reel.metadata.userAddress, cancellationToken);

            infoSidePanelView.SetPictureOwner(reel.metadata.userName, profile.face256SnapshotURL);
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
                    analytics.DeletePhoto();
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

        private void DownloadScreenshot()
        {
            browserBridge.OpenUrl(currentScreenshot.url);
            analytics.DownloadPhoto("Explorer");
        }

        private void CopyScreenshotLink()
        {
            var url = $"https://reels.decentraland.{(environmentProviderService.IsProd() ? "org" : "zone")}/{currentScreenshot.id}";

            clipboard.WriteText(url);
            browserBridge.OpenUrl(url);
            analytics.Share("Explorer", "Copy");
        }

        private void ShareOnTwitter()
        {
            var description = "Check out what I'm doing in Decentraland right now and join me!";
            var url = $"https://reels.decentraland.{(environmentProviderService.IsProd() ? "org" : "zone")}/{currentScreenshot.id}";
            var twitterUrl = $"https://twitter.com/intent/tweet?text={description}&hashtags=DCLCamera&url={url}";

            clipboard.WriteText(twitterUrl);
            browserBridge.OpenUrl(twitterUrl);
            analytics.Share("Explorer", "Twitter");
        }

        private void JumpInScene()
        {
            if (!int.TryParse(currentScreenshot.metadata.scene.location.x, out int x)
                || !int.TryParse(currentScreenshot.metadata.scene.location.y, out int y))
                return;

            void TrackToAnalyticsThenCloseView()
            {
                analytics.JumpIn("Explorer");
                view.Hide();
                dataStore.exploreV2.isOpen.Set(false);
            }

            dataStore.HUDs.gotoPanelVisible.Set(true, true);
            dataStore.HUDs.gotoPanelCoordinates.Set((new ParcelCoordinates(x, y), currentScreenshot.metadata.realm, TrackToAnalyticsThenCloseView), true);
        }

        private void OpenPictureOwnerProfile()
        {
            dataStore.HUDs.currentPlayerId.Set((currentScreenshot.metadata.userAddress, SCREEN_SOURCE));
        }
    }
}
