﻿using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using DCLFeatures.CameraReel.Gallery;
using DCLFeatures.CameraReel.ScreenshotViewer;
using DCLServices.CameraReelService;
using System;
using System.Threading;

namespace DCLFeatures.CameraReel.Section
{
    public class CameraReelSectionController : IDisposable
    {
        private const int LIMIT = 20;

        private readonly CameraReelModel cameraReelModel;
        private readonly CameraReelSectionView sectionView;
        private readonly CameraReelGalleryView galleryView;
        private readonly CameraReelGalleryStorageView galleryStorageView;
        private readonly DataStore dataStore;
        private readonly ICameraReelService service;
        private readonly Func<ScreenshotViewerController> screenshotViewerControllerFactory;

        private ScreenshotViewerController screenshotViewerController;
        private bool isUpdating;
        private int offset;
        private CancellationTokenSource fetchScreenshotsCancellationToken;

        public CameraReelSectionController(CameraReelSectionView sectionView,
            CameraReelGalleryView galleryView,
            CameraReelGalleryStorageView galleryStorageView,
            DataStore dataStore,
            ICameraReelService service,
            CameraReelModel cameraReelModel,
            Func<ScreenshotViewerController> screenshotViewerControllerFactory)
        {
            this.sectionView = sectionView;
            this.galleryStorageView = galleryStorageView;
            this.dataStore = dataStore;
            this.service = service;
            this.screenshotViewerControllerFactory = screenshotViewerControllerFactory;
            this.galleryView = galleryView;
            this.cameraReelModel = cameraReelModel;

            cameraReelModel.ScreenshotRemoved += OnScreenshotRemoved;
            cameraReelModel.ScreenshotAdded += OnScreenshotAdded;
            cameraReelModel.StorageUpdated += OnStorageUpdated;

            dataStore.HUDs.cameraReelSectionVisible.OnChange += SwitchGalleryVisibility;

            galleryView.ShowMoreButtonClicked += OnShowMoreClicked;
            galleryView.ScreenshotThumbnailClicked += ShowScreenshotWithMetadata;
        }

        public void Dispose()
        {
            cameraReelModel.ScreenshotRemoved -= OnScreenshotRemoved;
            cameraReelModel.ScreenshotAdded -= OnScreenshotAdded;
            cameraReelModel.StorageUpdated -= OnStorageUpdated;

            dataStore.HUDs.cameraReelSectionVisible.OnChange -= SwitchGalleryVisibility;
            galleryView.ShowMoreButtonClicked -= OnShowMoreClicked;
            galleryView.ScreenshotThumbnailClicked -= ShowScreenshotWithMetadata;
        }

        private void SwitchGalleryVisibility(bool isVisible, bool _)
        {
            sectionView.SwitchVisibility(isVisible);
            UpdateEmptyStateVisibility();

            if (!isUpdating)
                FetchScreenshots(0);
        }

        private void OnScreenshotAdded(bool isFirst, CameraReelResponse screenshot)
        {
            galleryView.AddScreenshotThumbnail(screenshot, isFirst);
            UpdateEmptyStateVisibility();
        }

        private void OnScreenshotRemoved(CameraReelResponse screenshot)
        {
            galleryView.DeleteScreenshotThumbnail(screenshot);
            UpdateEmptyStateVisibility();
        }

        private void ShowScreenshotWithMetadata(CameraReelResponse reelResponse)
        {
            screenshotViewerController ??= screenshotViewerControllerFactory.Invoke();
            screenshotViewerController.Show(reelResponse);
        }

        private void FetchScreenshots(int offset)
        {
            async UniTaskVoid FetchScreenshotsAndUpdateUiAsync(int offset, CancellationToken cancellationToken)
            {
                this.offset = offset;
                isUpdating = true;

                CameraReelResponses screenshots = await service.GetScreenshotGallery(
                    dataStore.player.ownPlayer.Get().id, LIMIT, offset, cancellationToken);

                isUpdating = false;
                this.offset += LIMIT;

                foreach (CameraReelResponse reel in screenshots.images)
                    cameraReelModel.AddScreenshotAsLast(reel);

                cameraReelModel.SetStorageStatus(screenshots.currentImages, screenshots.maxImages);
                sectionView.HideLoading();
                galleryView.SwitchVisibility(true);
            }

            fetchScreenshotsCancellationToken = fetchScreenshotsCancellationToken.SafeRestart();
            FetchScreenshotsAndUpdateUiAsync(offset, fetchScreenshotsCancellationToken.Token).Forget();
        }

        private void UpdateEmptyStateVisibility() =>
            galleryView.SwitchEmptyStateVisibility(cameraReelModel.LoadedScreenshotCount == 0);

        private void UpdateShowMoreVisibility() =>
            galleryView.SwitchShowMoreVisibility(cameraReelModel.TotalScreenshotsInStorage > cameraReelModel.LoadedScreenshotCount);

        private void OnShowMoreClicked() =>
            FetchScreenshots(offset);

        private void OnStorageUpdated(int totalScreenshots, int maxScreenshots)
        {
            galleryStorageView.UpdateStorageBar(totalScreenshots, maxScreenshots);
            UpdateShowMoreVisibility();
        }
    }
}