using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCLFeatures.CameraReel.Gallery;
using DCLFeatures.CameraReel.Section;
using DCLServices.CameraReelService;
using DCLServices.EnvironmentProvider;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Threading;
using UnityEngine;

namespace DCLFeatures.CameraReel.Tests
{
    public class ThumbnailContextMenuControllerShould
    {
        private IThumbnailContextMenuView view;
        private IClipboard clipboard;
        private IBrowserBridge browserBridge;
        private CameraReelModel cameraReelModel;
        private ICameraReelStorageService cameraReelStorageService;
        private DataStore dataStore;
        private ICameraReelAnalyticsService analytics;
        private IEnvironmentProviderService environmentProvider;
        private ThumbnailContextMenuController controller;
        private CameraReelResponse picture;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IThumbnailContextMenuView>();
            clipboard = Substitute.For<IClipboard>();
            browserBridge = Substitute.For<IBrowserBridge>();
            cameraReelStorageService = Substitute.For<ICameraReelStorageService>();
            dataStore = new DataStore();
            analytics = Substitute.For<ICameraReelAnalyticsService>();
            environmentProvider = Substitute.For<IEnvironmentProviderService>();

            var date = new DateTime(2020, 4, 25, 16, 43, 13);

            picture = new CameraReelResponse
            {
                id = "pictureId",
                metadata = new ScreenshotMetadata
                {
                    realm = "realm",
                    dateTime = new DateTimeOffset(date).ToUnixTimeSeconds().ToString(),
                    scene = new Scene
                    {
                        name = "scene",
                        location = new Location(new Vector2Int(15, 71)),
                    },
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    visiblePeople = new[]
                    {
                        new VisiblePerson
                        {
                            userAddress = "pictureOwnerId",
                            userName = "pictureOwnerName",
                            wearables = new[] {"w1", "w2"},
                            isGuest = false,
                        },
                    },
                },
            };

            cameraReelModel = new CameraReelModel();
            cameraReelModel.AddScreenshotAsFirst(picture, new CameraReelStorageStatus(1,110));

            controller = new ThumbnailContextMenuController(
                view, clipboard, cameraReelModel, browserBridge, cameraReelStorageService, dataStore,
                analytics, environmentProvider);

            view.OnSetup += Raise.Event<Action<CameraReelResponse>>(picture);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [TestCase(true, "https://reels.decentraland.org/pictureId")]
        [TestCase(false, "https://reels.decentraland.zone/pictureId")]
        public void CopyPictureLinkToClipboard(bool isProd, string expectedLink)
        {
            environmentProvider.IsProd().Returns(isProd);

            view.OnCopyPictureLinkRequested += Raise.Event<Action>();

            clipboard.Received(1).WriteText(expectedLink);
        }

        [Test]
        public void DownloadPicture()
        {
            view.OnDownloadRequested += Raise.Event<Action>();

            browserBridge.Received(1).OpenUrl(picture.url);
        }

        [TestCase(true, "https://twitter.com/intent/tweet?text=Check out what I'm doing in Decentraland right now and join me!&hashtags=DCLCamera&url=https://reels.decentraland.org/pictureId")]
        [TestCase(false, "https://twitter.com/intent/tweet?text=Check out what I'm doing in Decentraland right now and join me!&hashtags=DCLCamera&url=https://reels.decentraland.zone/pictureId")]
        public void ShareOnTwitter(bool isProd, string expectedLink)
        {
            environmentProvider.IsProd().Returns(isProd);

            view.OnShareToTwitterRequested += Raise.Event<Action>();

            clipboard.Received(1).WriteText(expectedLink);
            browserBridge.Received(1).OpenUrl(expectedLink);
        }

        [Test]
        public void DeletePictureWhenNotificationIsConfirmed()
        {
            cameraReelStorageService.DeleteScreenshot(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                    .Returns(UniTask.FromResult(new CameraReelStorageStatus(15, 500)));

            void ConfirmDeletion(GenericConfirmationNotificationData notification, GenericConfirmationNotificationData _) =>
                notification.ConfirmAction.Invoke();

            dataStore.notifications.GenericConfirmation.OnChange += ConfirmDeletion;

            view.OnDeletePictureRequested += Raise.Event<Action>();

            dataStore.notifications.GenericConfirmation.OnChange -= ConfirmDeletion;

            cameraReelStorageService.Received(1).DeleteScreenshot("pictureId", Arg.Any<CancellationToken>());
            Assert.AreEqual(15, cameraReelModel.TotalScreenshotsInStorage);
            Assert.AreEqual(500, cameraReelModel.MaxScreenshotsInStorage);
            Assert.AreEqual(0, cameraReelModel.LoadedScreenshotCount);
        }

        [Test]
        public void DontDeletePictureWhenNotificationIsCancelled()
        {
            cameraReelStorageService.DeleteScreenshot(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                    .Returns(UniTask.FromResult(new CameraReelStorageStatus(15, 500)));

            void CancelDeletion(GenericConfirmationNotificationData notification, GenericConfirmationNotificationData _) =>
                notification.CancelAction.Invoke();

            dataStore.notifications.GenericConfirmation.OnChange += CancelDeletion;

            view.OnDeletePictureRequested += Raise.Event<Action>();

            dataStore.notifications.GenericConfirmation.OnChange -= CancelDeletion;

            cameraReelStorageService.DidNotReceiveWithAnyArgs().DeleteScreenshot(default);
            Assert.AreEqual(0, cameraReelModel.TotalScreenshotsInStorage);
            Assert.AreEqual(0, cameraReelModel.MaxScreenshotsInStorage);
            Assert.AreEqual(1, cameraReelModel.LoadedScreenshotCount);
        }
    }
}
