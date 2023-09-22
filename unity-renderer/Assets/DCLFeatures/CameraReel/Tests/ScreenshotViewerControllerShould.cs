using Cysharp.Threading.Tasks;
using DCL;
using DCL.Browser;
using DCLFeatures.CameraReel.ScreenshotViewer;
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
    public class ScreenshotViewerControllerShould
    {
        private IScreenshotViewerView screenshotViewerView;
        private ScreenshotViewerController controller;
        private IScreenshotViewerInfoSidePanelView infoSidePanelView;
        private IScreenshotViewerActionsPanelView actionsPanelView;
        private IUserProfileBridge userProfileBridge;
        private CameraReelModel cameraReelModel;
        private IClipboard clipboard;
        private IBrowserBridge browserBridge;
        private IEnvironmentProviderService environmentProviderService;
        private ICameraReelStorageService cameraReelStorageService;
        private DataStore dataStore;

        [SetUp]
        public void SetUp()
        {
            screenshotViewerView = Substitute.For<IScreenshotViewerView>();
            infoSidePanelView = Substitute.For<IScreenshotViewerInfoSidePanelView>();
            actionsPanelView = Substitute.For<IScreenshotViewerActionsPanelView>();

            UserProfile pictureOwnerProfile = ScriptableObject.CreateInstance<UserProfile>();

            pictureOwnerProfile.UpdateData(new UserProfileModel
            {
                userId = "pictureOwnerId",
                snapshots = new UserProfileModel.Snapshots
                {
                    face256 = "face256",
                },
                avatar = new AvatarModel(),
            });

            userProfileBridge = Substitute.For<IUserProfileBridge>();
            userProfileBridge.Get("pictureOwnerId").Returns(pictureOwnerProfile);

            cameraReelModel = new CameraReelModel();
            clipboard = Substitute.For<IClipboard>();
            browserBridge = Substitute.For<IBrowserBridge>();
            environmentProviderService = Substitute.For<IEnvironmentProviderService>();
            cameraReelStorageService = Substitute.For<ICameraReelStorageService>();
            dataStore = new DataStore();

            controller = new ScreenshotViewerController(screenshotViewerView,
                cameraReelModel, dataStore, cameraReelStorageService,
                userProfileBridge, clipboard,
                browserBridge, Substitute.For<ICameraReelAnalyticsService>(),
                environmentProviderService, actionsPanelView,
                infoSidePanelView);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void ShowAllTheInformationOfThePicture()
        {
            var date = new DateTime(2020, 4, 25, 16, 43, 13);

            var scene = new Scene
            {
                name = "scene",
                location = new Location(new Vector2Int(15, 71)),
            };

            VisiblePerson[] people =
            {
                new ()
                {
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    wearables = new[] { "w1", "w2" },
                    isGuest = false,
                },
                new ()
                {
                    userAddress = "otherUser1",
                    userName = "user1",
                    wearables = new[] { "w3", "w4" },
                    isGuest = false,
                },
                new ()
                {
                    userAddress = "guestUser",
                    userName = "user2",
                    wearables = Array.Empty<string>(),
                    isGuest = true,
                },
            };

            var picture = new CameraReelResponse
            {
                id = "pictureId",
                url = "pictureUrl",
                metadata = new ScreenshotMetadata
                {
                    realm = "realm",
                    dateTime = new DateTimeOffset(date).ToUnixTimeSeconds().ToString(),
                    scene = scene,
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    visiblePeople = people,
                },
            };

            controller.Show(picture);

            VerifyPictureShown(scene, date, people, "pictureUrl", "pictureOwnerName", "face256");
        }

        [Test]
        public void Hide()
        {
            screenshotViewerView.CloseButtonClicked += Raise.Event<Action>();

            screenshotViewerView.Received(1).Hide();
        }

        [Test]
        public void NavigateThroughPictures()
        {
            var date1 = new DateTime(2020, 4, 25, 16, 43, 13);

            var scene1 = new Scene
            {
                name = "scene1",
                location = new Location(new Vector2Int(15, 71)),
            };

            VisiblePerson[] people1 =
            {
                new ()
                {
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    wearables = new[] { "w1", "w2" },
                    isGuest = false,
                },
                new ()
                {
                    userAddress = "otherUser1",
                    userName = "user1",
                    wearables = new[] { "w3", "w4" },
                    isGuest = false,
                },
                new ()
                {
                    userAddress = "guestUser",
                    userName = "user2",
                    wearables = Array.Empty<string>(),
                    isGuest = true,
                },
            };

            var picture1 = new CameraReelResponse
            {
                id = "pic1",
                url = "pictureUrl1",
                metadata = new ScreenshotMetadata
                {
                    realm = "realm",
                    dateTime = new DateTimeOffset(date1).ToUnixTimeSeconds().ToString(),
                    scene = scene1,
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    visiblePeople = people1,
                },
            };

            var date2 = new DateTime(2020, 7, 11, 12, 34, 46);

            var scene2 = new Scene
            {
                name = "scene2",
                location = new Location(new Vector2Int(15, 71)),
            };

            VisiblePerson[] people2 =
            {
                new ()
                {
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    wearables = new[] { "w1", "w2" },
                    isGuest = false,
                },
            };

            var picture2 = new CameraReelResponse
            {
                id = "pic2",
                url = "pictureUrl2",
                metadata = new ScreenshotMetadata
                {
                    realm = "realm",
                    dateTime = new DateTimeOffset(date2).ToUnixTimeSeconds().ToString(),
                    scene = scene2,
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    visiblePeople = people2,
                },
            };

            cameraReelModel.AddScreenshotAsLast(picture1);
            cameraReelModel.AddScreenshotAsLast(picture2);

            controller.Show(picture2);
            screenshotViewerView.ClearReceivedCalls();
            infoSidePanelView.ClearReceivedCalls();

            screenshotViewerView.PrevScreenshotClicked += Raise.Event<Action>();

            VerifyPictureShown(scene1, date1, people1, "pictureUrl1", "pictureOwnerName", "face256");

            screenshotViewerView.ClearReceivedCalls();
            infoSidePanelView.ClearReceivedCalls();

            screenshotViewerView.NextScreenshotClicked += Raise.Event<Action>();

            VerifyPictureShown(scene2, date2, people2, "pictureUrl2", "pictureOwnerName", "face256");
        }

        [Test]
        public void DownloadPicture()
        {
            var date = new DateTime(2020, 4, 25, 16, 43, 13);

            var scene = new Scene
            {
                name = "scene",
                location = new Location(new Vector2Int(15, 71)),
            };

            VisiblePerson[] people =
            {
                new ()
                {
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    wearables = new[] { "w1", "w2" },
                    isGuest = false,
                },
                new ()
                {
                    userAddress = "otherUser1",
                    userName = "user1",
                    wearables = new[] { "w3", "w4" },
                    isGuest = false,
                },
                new ()
                {
                    userAddress = "guestUser",
                    userName = "user2",
                    wearables = Array.Empty<string>(),
                    isGuest = true,
                },
            };

            var picture = new CameraReelResponse
            {
                id = "pictureId",
                url = "pictureUrl",
                metadata = new ScreenshotMetadata
                {
                    realm = "realm",
                    dateTime = new DateTimeOffset(date).ToUnixTimeSeconds().ToString(),
                    scene = scene,
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    visiblePeople = people,
                },
            };

            controller.Show(picture);

            actionsPanelView.DownloadClicked += Raise.Event<Action>();

            browserBridge.Received(1).OpenUrl("pictureUrl");
        }

        [TestCase(true, "https://reels.decentraland.org/pictureId")]
        [TestCase(false, "https://reels.decentraland.zone/pictureId")]
        public void CopyLinkOfPicture(bool isProd, string expectedLink)
        {
            var picture = new CameraReelResponse
            {
                id = "pictureId",
                url = "pictureUrl",
                metadata = new ScreenshotMetadata
                {
                    realm = "realm",
                    dateTime = "",
                    scene = new Scene
                    {
                        name = "scene",
                        location = new Location(new Vector2Int(15, 71)),
                    },
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    visiblePeople = Array.Empty<VisiblePerson>(),
                },
            };

            controller.Show(picture);
            environmentProviderService.IsProd().Returns(isProd);

            actionsPanelView.LinkClicked += Raise.Event<Action>();

            clipboard.Received(1).WriteText(expectedLink);
            browserBridge.Received(1).OpenUrl(expectedLink);
        }

        [TestCase(true, "https://twitter.com/intent/tweet?text=Check out what I'm doing in Decentraland right now and join me!&hashtags=DCLCamera&url=https://reels.decentraland.org/pictureId")]
        [TestCase(false, "https://twitter.com/intent/tweet?text=Check out what I'm doing in Decentraland right now and join me!&hashtags=DCLCamera&url=https://reels.decentraland.zone/pictureId")]
        public void ShareOnTwitter(bool isProd, string expectedLink)
        {
            var picture = new CameraReelResponse
            {
                id = "pictureId",
                url = "pictureUrl",
                metadata = new ScreenshotMetadata
                {
                    realm = "realm",
                    dateTime = "",
                    scene = new Scene
                    {
                        name = "scene",
                        location = new Location(new Vector2Int(15, 71)),
                    },
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    visiblePeople = Array.Empty<VisiblePerson>(),
                },
            };

            controller.Show(picture);
            environmentProviderService.IsProd().Returns(isProd);

            actionsPanelView.TwitterClicked += Raise.Event<Action>();

            clipboard.Received(1).WriteText(expectedLink);
            browserBridge.Received(1).OpenUrl(expectedLink);
        }

        [Test]
        public void DeletePictureWhenNotificationIsConfirmed()
        {
            var picture = new CameraReelResponse
            {
                id = "pictureId",
                url = "pictureUrl",
                metadata = new ScreenshotMetadata
                {
                    realm = "realm",
                    dateTime = "",
                    scene = new Scene
                    {
                        name = "scene",
                        location = new Location(new Vector2Int(15, 71)),
                    },
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    visiblePeople = Array.Empty<VisiblePerson>(),
                },
            };

            controller.Show(picture);

            cameraReelStorageService.DeleteScreenshot(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                    .Returns(UniTask.FromResult(new CameraReelStorageStatus(15, 500)));

            void ConfirmDeletion(GenericConfirmationNotificationData notification, GenericConfirmationNotificationData _) =>
                notification.ConfirmAction.Invoke();

            dataStore.notifications.GenericConfirmation.OnChange += ConfirmDeletion;

            actionsPanelView.DeleteClicked += Raise.Event<Action>();

            dataStore.notifications.GenericConfirmation.OnChange -= ConfirmDeletion;

            cameraReelStorageService.Received(1).DeleteScreenshot("pictureId", Arg.Any<CancellationToken>());
            Assert.AreEqual(15, cameraReelModel.TotalScreenshotsInStorage);
            Assert.AreEqual(500, cameraReelModel.MaxScreenshotsInStorage);
            Assert.AreEqual(0, cameraReelModel.LoadedScreenshotCount);
        }

        [Test]
        public void DontDeletePictureWhenNotificationIsCancelled()
        {
            void CancelDeletion(GenericConfirmationNotificationData notification, GenericConfirmationNotificationData _) =>
                notification.CancelAction.Invoke();

            var picture = new CameraReelResponse
            {
                id = "pictureId",
                url = "pictureUrl",
                metadata = new ScreenshotMetadata
                {
                    realm = "realm",
                    dateTime = "0",
                    scene = new Scene
                    {
                        name = "scene",
                        location = new Location(new Vector2Int(15, 71)),
                    },
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    visiblePeople = Array.Empty<VisiblePerson>(),
                },
            };

            controller.Show(picture);

            cameraReelStorageService.DeleteScreenshot(Arg.Any<string>(), Arg.Any<CancellationToken>())
                                    .Returns(UniTask.FromResult(new CameraReelStorageStatus(15, 500)));

            dataStore.notifications.GenericConfirmation.OnChange += CancelDeletion;

            actionsPanelView.DeleteClicked += Raise.Event<Action>();

            dataStore.notifications.GenericConfirmation.OnChange -= CancelDeletion;

            cameraReelStorageService.DidNotReceiveWithAnyArgs().DeleteScreenshot(default);
            Assert.AreEqual(0, cameraReelModel.TotalScreenshotsInStorage);
            Assert.AreEqual(0, cameraReelModel.MaxScreenshotsInStorage);
            Assert.AreEqual(0, cameraReelModel.LoadedScreenshotCount);
        }

        [Test]
        public void JumpIn()
        {
            var picture = new CameraReelResponse
            {
                id = "pictureId",
                url = "pictureUrl",
                metadata = new ScreenshotMetadata
                {
                    realm = "realm",
                    dateTime = "0",
                    scene = new Scene
                    {
                        name = "scene",
                        location = new Location(new Vector2Int(15, 71)),
                    },
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    visiblePeople = Array.Empty<VisiblePerson>(),
                },
            };

            controller.Show(picture);

            infoSidePanelView.SceneButtonClicked += Raise.Event<Action>();

            Assert.IsTrue(dataStore.HUDs.gotoPanelVisible.Get());
            Assert.AreEqual(new ParcelCoordinates(15, 71).ToString(), dataStore.HUDs.gotoPanelCoordinates.Get().coordinates.ToString());
            Assert.AreEqual("realm", dataStore.HUDs.gotoPanelCoordinates.Get().realm);
        }

        [Test]
        public void OpenPictureOwnerProfile()
        {
            var picture = new CameraReelResponse
            {
                id = "pictureId",
                url = "pictureUrl",
                metadata = new ScreenshotMetadata
                {
                    realm = "realm",
                    dateTime = "0",
                    scene = new Scene
                    {
                        name = "scene",
                        location = new Location(new Vector2Int(15, 71)),
                    },
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    visiblePeople = Array.Empty<VisiblePerson>(),
                },
            };

            controller.Show(picture);

            infoSidePanelView.OnOpenPictureOwnerProfile += Raise.Event<Action>();

            Assert.AreEqual("pictureOwnerId", dataStore.HUDs.currentPlayerId.Get().playerId);
        }

        private void VerifyPictureShown(Scene scene, DateTime date, VisiblePerson[] people, string url,
            string ownerUserName, string ownerAvatarUrl)
        {
            screenshotViewerView.SetScreenshotImage(url);
            screenshotViewerView.Received(1).Show();
            infoSidePanelView.Received(1).SetSceneInfoText(scene);
            infoSidePanelView.Received(1).SetDateText(date);
            infoSidePanelView.Received(1).ShowVisiblePersons(people);
            infoSidePanelView.Received(1).SetPictureOwner(ownerUserName, ownerAvatarUrl);
        }
    }
}
