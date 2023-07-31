﻿using CameraReel.Gallery;
using DCL;
using DCL.Helpers;
using DCLServices.CameraReelService;
using Features.CameraReel.ScreenshotViewer;
using System;
using Object = UnityEngine.Object;

namespace Features.CameraReel
{
    public class CameraReelSectionController : IDisposable
    {
        private readonly CameraReelModel cameraReelModel;

        private readonly CameraReelSectionView sectionView;
        private readonly CameraReelGalleryView galleryView;
        private readonly CameraReelGalleryStorageView galleryStorageView;

        private ScreenshotViewerHUDView screenshotViewerView;
        private ScreenshotViewerController screenshotViewerController;

        private bool firstLoad = true;

        public CameraReelSectionController(CameraReelSectionView sectionView, CameraReelGalleryView galleryView, CameraReelGalleryStorageView galleryStorageView)
        {
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

            DataStore.i.HUDs.cameraReelSectionVisible.OnChange += SwitchGalleryVisibility;

            galleryView.ShowMoreButtonClicked += cameraReelModel.LoadImagesAsync;
            galleryView.ScreenshotThumbnailClicked += ShowScreenshotWithMetadata;
        }

        public void Dispose()
        {
            cameraReelModel.Updated -= OnModelUpdated;

            DataStore.i.HUDs.cameraReelSectionVisible.OnChange -= SwitchGalleryVisibility;
            galleryView.ShowMoreButtonClicked -= cameraReelModel.LoadImagesAsync;
            galleryView.ScreenshotThumbnailClicked -= ShowScreenshotWithMetadata;

            Utils.SafeDestroy(screenshotViewerView);
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

        private void ShowScreenshotWithMetadata(CameraReelResponse reelResponse)
        {
            if (screenshotViewerView == null)
            {
                screenshotViewerView = Object.Instantiate(sectionView.ScreenshotViewerPrefab);
                screenshotViewerController = new ScreenshotViewerController(screenshotViewerView, cameraReelModel);
                screenshotViewerController.Initialize();
            }

            screenshotViewerController.Show(reelResponse);
        }
    }
}
