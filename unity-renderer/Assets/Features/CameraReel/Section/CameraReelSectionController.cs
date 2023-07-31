﻿using CameraReel.Gallery;
using DCL;
using DCL.Helpers;
using DCL.Providers;
using DCLServices.CameraReelService;
using Features.CameraReel.ScreenshotViewer;
using System;
using System.Threading.Tasks;

namespace Features.CameraReel
{
    public class CameraReelSectionController : IDisposable
    {
        private const string SCREENSHOT_VIEW = "ScreenshotView";

        private readonly IAddressableResourceProvider assetProvider;
        private readonly CameraReelModel cameraReelModel;

        private readonly CameraReelSectionView sectionView;
        private readonly CameraReelGalleryView galleryView;
        private readonly CameraReelGalleryStorageView galleryStorageView;
        private ScreenshotViewerHUDView screenshotViewer;

        private bool firstLoad = true;

        public CameraReelSectionController(IAddressableResourceProvider assetProvider, CameraReelSectionView sectionView, CameraReelGalleryView galleryView, CameraReelGalleryStorageView galleryStorageView)
        {
            this.assetProvider = assetProvider;

            // Views
            this.sectionView = sectionView;
            this.galleryView = galleryView;
            this.galleryStorageView = galleryStorageView;

            // Model
            cameraReelModel = new CameraReelModel();
        }

        public void Initialize()
        {
            cameraReelModel.Updated += OnModelUpdated;

            DataStore.i.HUDs.cameraReelVisible.OnChange += SwitchGalleryVisibility;

            galleryView.ShowMoreButtonClicked += cameraReelModel.LoadImagesAsync;
            galleryView.ScreenshotThumbnailClicked += ShowScreenshotWithMetadata;
        }

        public void Dispose()
        {
            cameraReelModel.Updated -= OnModelUpdated;

            DataStore.i.HUDs.cameraReelVisible.OnChange -= SwitchGalleryVisibility;
            galleryView.ShowMoreButtonClicked -= cameraReelModel.LoadImagesAsync;
            galleryView.ScreenshotThumbnailClicked -= ShowScreenshotWithMetadata;

            if (screenshotViewer != null)
            {
                screenshotViewer.PrevScreenshotClicked -= ShowPrevScreenshot;
                screenshotViewer.NextScreenshotClicked -= ShowNextScreenshot;
            }

            Utils.SafeDestroy(screenshotViewer);
        }

        private void OnModelUpdated(CameraReelResponses reelResponses)
        {
            galleryStorageView.UpdateStorageBar(reelResponses.currentImages, reelResponses.maxImages);

            if (firstLoad)
            {
                sectionView.ShowGalleryWhenLoaded();
                firstLoad = false;
            }

            galleryView.DownloadImageAndCreateObject(reelResponses.images);
        }

        private void SwitchGalleryVisibility(bool isVisible, bool _)
        {
            sectionView.SwitchVisibility(isVisible);

            if (firstLoad && !cameraReelModel.IsUpdating)
                cameraReelModel.LoadImagesAsync();
        }

        private async void ShowScreenshotWithMetadata(CameraReelResponse reelResponse)
        {
            if (screenshotViewer == null)
            {
                screenshotViewer = await assetProvider.Instantiate<ScreenshotViewerHUDView>(SCREENSHOT_VIEW, SCREENSHOT_VIEW);
                screenshotViewer.PrevScreenshotClicked += ShowPrevScreenshot;
                screenshotViewer.NextScreenshotClicked += ShowNextScreenshot;
            }

            screenshotViewer.Show(reelResponse);
        }

        private void ShowNextScreenshot(CameraReelResponse current)
        {
            CameraReelResponse next = cameraReelModel.GetNextScreenshot(current);

            if (next != null)
                ShowScreenshotWithMetadata(next);
        }

        private void ShowPrevScreenshot(CameraReelResponse current)
        {
            CameraReelResponse prev = cameraReelModel.GetPreviousScreenshot(current);

            if (prev != null)
                ShowScreenshotWithMetadata(prev);
        }
    }
}