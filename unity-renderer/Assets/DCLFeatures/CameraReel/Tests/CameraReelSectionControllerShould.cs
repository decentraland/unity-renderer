using Cysharp.Threading.Tasks;
using DCL;
using DCLFeatures.CameraReel.Gallery;
using DCLFeatures.CameraReel.Section;
using DCLServices.CameraReelService;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCLFeatures.CameraReel.Tests
{
    public class CameraReelSectionControllerShould
    {
        private ICameraReelSectionView sectionView;
        private ICameraReelGalleryView galleryView;
        private ICameraReelGalleryStorageView storageView;
        private ICameraReelStorageService storageService;
        private DataStore dataStore;
        private CameraReelModel cameraReelModel;
        private ICameraReelAnalyticsService analytics;
        private CameraReelSectionController controller;
        private CameraReelResponse picture;

        [SetUp]
        public void SetUp()
        {
            sectionView = Substitute.For<ICameraReelSectionView>();
            galleryView = Substitute.For<ICameraReelGalleryView>();
            storageView = Substitute.For<ICameraReelGalleryStorageView>();
            storageService = Substitute.For<ICameraReelStorageService>();
            dataStore = new DataStore();
            cameraReelModel = new CameraReelModel();
            analytics = Substitute.For<ICameraReelAnalyticsService>();

            controller = new CameraReelSectionController(
                sectionView, galleryView, storageView, dataStore, storageService, cameraReelModel,
                () => null, analytics);

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
                            wearables = new[] { "w1", "w2" },
                            isGuest = false,
                        },
                        new VisiblePerson
                        {
                            userAddress = "otherUser1",
                            userName = "user1",
                            wearables = new[] { "w3", "w4" },
                            isGuest = false,
                        },
                        new VisiblePerson
                        {
                            userAddress = "guestUser",
                            userName = "user2",
                            wearables = Array.Empty<string>(),
                            isGuest = true,
                        },
                    },
                },
            };
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [Test]
        public void AddThumbnailToGallery()
        {
            cameraReelModel.AddScreenshotAsFirst(picture, new CameraReelStorageStatus(1,100));

            galleryView.Received(1).AddScreenshotThumbnail(picture, true);
            galleryView.Received(1).SwitchEmptyStateVisibility(false);
        }

        [Test]
        public void RemoveThumbnailFromGallery()
        {
            cameraReelModel.RemoveScreenshot(picture);

            galleryView.Received(1).DeleteScreenshotThumbnail(picture);
            galleryView.Received(1).SwitchEmptyStateVisibility(true);
        }

        [Test]
        public void UpdateStorage()
        {
            cameraReelModel.SetStorageStatus(13, 300);

            storageView.Received(1).UpdateStorageBar(13, 300);
        }

        [Test]
        public void FetchPicturesAndShowThem()
        {
            dataStore.player.ownPlayer.Set(new Player
            {
                id = "myUserId",
            });

            storageService.GetScreenshotGallery("myUserId", 30, 0, Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromResult(new CameraReelResponses
                           {
                               images = new List<CameraReelResponse> { picture },
                               currentImages = 17,
                               maxImages = 250,
                           }));

            dataStore.HUDs.cameraReelSectionVisible.Set(true, true);

            sectionView.Received(1).SwitchVisibility(true);
            galleryView.Received(1).SwitchEmptyStateVisibility(false);
            storageService.Received(1).GetScreenshotGallery("myUserId", 30, 0, Arg.Any<CancellationToken>());
            galleryView.Received(1).AddScreenshotThumbnail(picture, false);
            storageView.Received(1).UpdateStorageBar(17, 250);
            sectionView.Received(1).HideLoading();
            galleryView.Received(1).SwitchVisibility(true);
        }

        [Test]
        public void FetchNextPageOfPictures()
        {
            dataStore.player.ownPlayer.Set(new Player
            {
                id = "myUserId",
            });

            storageService.GetScreenshotGallery("myUserId", 30, 0, Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromResult(new CameraReelResponses
                           {
                               images = new List<CameraReelResponse> { picture },
                               currentImages = 17,
                               maxImages = 250,
                           }));

            CameraReelResponse otherPicture = new()
            {
                id = "pic2",
                metadata = new ScreenshotMetadata
                {
                    realm = "realm",
                    dateTime = new DateTimeOffset(new DateTime(2020, 4, 16)).ToUnixTimeSeconds().ToString(),
                    scene = new Scene
                    {
                        name = "scene",
                        location = new Location(new Vector2Int(11, 2)),
                    },
                    userAddress = "pictureOwnerId",
                    userName = "pictureOwnerName",
                    visiblePeople = Array.Empty<VisiblePerson>(),
                },
            };

            storageService.GetScreenshotGallery("myUserId", 30, 30, Arg.Any<CancellationToken>())
                          .Returns(UniTask.FromResult(new CameraReelResponses
                           {
                               images = new List<CameraReelResponse>
                               {
                                   otherPicture,
                               },
                               currentImages = 17,
                               maxImages = 250,
                           }));

            galleryView.ShowMoreButtonClicked += Raise.Event<Action>();
            galleryView.ShowMoreButtonClicked += Raise.Event<Action>();

            storageService.Received(1).GetScreenshotGallery("myUserId", 30, 0, Arg.Any<CancellationToken>());
            storageService.Received(1).GetScreenshotGallery("myUserId", 30, 30, Arg.Any<CancellationToken>());
            galleryView.Received(1).AddScreenshotThumbnail(picture, false);
            galleryView.Received(1).AddScreenshotThumbnail(otherPicture, false);
            storageView.Received(2).UpdateStorageBar(17, 250);
        }
    }
}
