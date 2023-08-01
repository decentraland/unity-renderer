using DCL;
using DCL.Helpers;
using DCLFeatures.CameraReel.Gallery;
using DCLFeatures.CameraReel.ScreenshotViewer;
using DCLServices.CameraReelService;
using System;
using Object = UnityEngine.Object;

namespace DCLFeatures.CameraReel.Section
{
    public class CameraReelSectionController : IDisposable
    {
        private readonly CameraReelModel cameraReelModel;
        private readonly CameraReelSectionView sectionView;

        private readonly CameraReelGalleryView galleryView;
        private readonly CameraReelGalleryStorageView galleryStorageView;

        private ScreenshotViewerView screenshotViewerView;
        private ScreenshotViewerController screenshotViewerController;

        private bool firstLoad = true;

        public CameraReelSectionController(CameraReelSectionView sectionView, CameraReelGalleryView galleryView, CameraReelGalleryStorageView galleryStorageView)
        {
            // Views
            this.sectionView = sectionView;

            this.galleryStorageView = galleryStorageView;
            this.galleryView = galleryView;

            // Model
            cameraReelModel = new CameraReelModel();
        }

        public void Initialize()
        {
            cameraReelModel.ScreenshotBatchFetched += OnModelScreenshotBatchFetched;
            cameraReelModel.ScreenshotRemoved += galleryView.DeleteScreenshotThumbnail;
            cameraReelModel.ScreenshotUploaded += galleryView.AddScreenshotThumbnail;

            DataStore.i.HUDs.cameraReelSectionVisible.OnChange += SwitchGalleryVisibility;

            galleryView.ShowMoreButtonClicked += cameraReelModel.RequestScreenshotsBatchAsync;
            galleryView.ScreenshotThumbnailClicked += ShowScreenshotWithMetadata;
        }

        public void Dispose()
        {
            cameraReelModel.ScreenshotBatchFetched -= OnModelScreenshotBatchFetched;
            cameraReelModel.ScreenshotRemoved -= galleryView.DeleteScreenshotThumbnail;
            cameraReelModel.ScreenshotUploaded -= galleryView.AddScreenshotThumbnail;

            DataStore.i.HUDs.cameraReelSectionVisible.OnChange -= SwitchGalleryVisibility;
            galleryView.ShowMoreButtonClicked -= cameraReelModel.RequestScreenshotsBatchAsync;
            galleryView.ScreenshotThumbnailClicked -= ShowScreenshotWithMetadata;

            Utils.SafeDestroy(screenshotViewerView);
        }

        private void OnModelScreenshotBatchFetched(CameraReelResponses reelResponses)
        {
            galleryStorageView.UpdateStorageBar(reelResponses.currentImages, reelResponses.maxImages);

            if (firstLoad)
            {
                sectionView.ShowGalleryWhenLoaded();
                firstLoad = false;
            }

            galleryView.AddScreenshotThumbnails(reelResponses.images);
        }

        private void SwitchGalleryVisibility(bool isVisible, bool _)
        {
            sectionView.SwitchVisibility(isVisible);

            if (firstLoad && !cameraReelModel.IsUpdating)
                cameraReelModel.RequestScreenshotsBatchAsync();
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
