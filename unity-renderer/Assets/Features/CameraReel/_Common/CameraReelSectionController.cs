using CameraReel.Gallery;
using CameraReel.ScreenshotViewer;
using DCL;
using DCL.Helpers;
using DCL.Providers;
using DCLServices.CameraReelService;
using System;

namespace Features.CameraReel
{
    public class CameraReelSectionController : IDisposable
    {
        private const string SCREENSHOT_VIEW = "ScreenshotView";

        private readonly IAddressableResourceProvider assetProvider;
        private readonly CameraReelSectionView sectionView;
        private readonly CameraReelGalleryView galleryView;
        private readonly CameraReelGalleryStorageView galleryStorageView;

        private ScreenshotViewerHUDView screenshotViewer;

        private bool firstLoad = true;

        public CameraReelSectionController(IAddressableResourceProvider assetProvider, CameraReelSectionView sectionView, CameraReelGalleryView galleryView, CameraReelGalleryStorageView galleryStorageView)
        {
            this.assetProvider = assetProvider;
            this.sectionView = sectionView;
            this.galleryView = galleryView;
            this.galleryStorageView = galleryStorageView;
        }

        public void Initialize()
        {
            DataStore.i.HUDs.cameraReelVisible.OnChange += SwitchGalleryVisibility;
            galleryView.ScreenshotsStorageUpdated += UpdateStorageBar;
            galleryView.ScreenshotThumbnailClicked += ShowScreenshotWithMetadata;
        }

        private async void ShowScreenshotWithMetadata(CameraReelResponse reelResponse)
        {
            if (screenshotViewer == null)
            {
                screenshotViewer = await assetProvider.Instantiate<ScreenshotViewerHUDView>(SCREENSHOT_VIEW, "ScreenshotViewer");
                screenshotViewer.PrevScreenshotClicked += ShowPrevScreenshot;
                screenshotViewer.NextScreenshotClicked += ShowNextScreenshot;
            }

            screenshotViewer.Show(reelResponse);
        }

        public void Dispose()
        {
            DataStore.i.HUDs.cameraReelVisible.OnChange -= SwitchGalleryVisibility;
            galleryView.ScreenshotsStorageUpdated -= UpdateStorageBar;
            galleryView.ScreenshotThumbnailClicked -= ShowScreenshotWithMetadata;

            if (screenshotViewer != null)
            {
                screenshotViewer.PrevScreenshotClicked -= ShowPrevScreenshot;
                screenshotViewer.NextScreenshotClicked -= ShowNextScreenshot;
            }

            Utils.SafeDestroy(screenshotViewer);
        }

        private void ShowNextScreenshot(CameraReelResponse current)
        {
            CameraReelResponse next = galleryView.GetNextScreenshot(current);

            if (next != null)
                ShowScreenshotWithMetadata(next);
        }

        private void ShowPrevScreenshot(CameraReelResponse current)
        {
            CameraReelResponse prev = galleryView.GetPreviousScreenshot(current);

            if (prev != null)
                ShowScreenshotWithMetadata(prev);
        }

        private void SwitchGalleryVisibility(bool isVisible, bool _)
        {
            sectionView.SwitchVisibility(isVisible);

            if (firstLoad)
            {
                galleryView.LoadImagesAsync();
                galleryView.ScreenshotsStorageUpdated += ShowGalleryWhenLoaded;
                firstLoad = false;
            }
        }

        private void ShowGalleryWhenLoaded((int current, int max) _)
        {
            sectionView.ShowGalleryWhenLoaded();
        }

        private void UpdateStorageBar((int current, int max) storage) =>
            galleryStorageView.UpdateStorageBar(storage.current, storage.max);
    }
}
