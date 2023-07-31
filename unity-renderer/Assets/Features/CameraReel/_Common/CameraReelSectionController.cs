using CameraReel.Gallery;
using CameraReel.ScreenshotViewer;
using DCL;
using DCL.Helpers;
using DCL.Providers;
using DCLServices.CameraReelService;
using System;
using System.Threading.Tasks;

namespace Features.CameraReel
{
    public class CameraReelSectionController : IDisposable
    {
        private const string SCREENSHOT_VIEW = "ScreenshotView";

        private readonly IAddressableResourceProvider assetProvider;
        private readonly CameraReelSectionView sectionView;
        private readonly CameraReelGalleryView galleryView;
        private readonly CameraReelGalleryStorageView galleryStorageView;

        private CameraReelModel cameraReelModel;
        private ScreenshotViewerHUDView screenshotViewer;

        private bool firstLoad = true;

        public CameraReelSectionController(IAddressableResourceProvider assetProvider, CameraReelSectionView sectionView, CameraReelGalleryView galleryView, CameraReelGalleryStorageView galleryStorageView)
        {
            this.assetProvider = assetProvider;

            // Views
            this.sectionView = sectionView;
            this.galleryView = galleryView;
            this.galleryStorageView = galleryStorageView;
        }

        public void Initialize()
        {
            cameraReelModel = new CameraReelModel();
            cameraReelModel.LoadedScreenshotsUpdated += OnModelUpdated;
            DataStore.i.HUDs.cameraReelVisible.OnChange += SwitchGalleryVisibility;
            // galleryView.ShowMoreButtonClicked += LoadMoreImages;
            // galleryView.ScreenshotThumbnailClicked += ShowScreenshotWithMetadata;
        }

        public void Dispose()
        {
            cameraReelModel.LoadedScreenshotsUpdated -= OnModelUpdated;
            DataStore.i.HUDs.cameraReelVisible.OnChange -= SwitchGalleryVisibility;

            // galleryView.ShowMoreButtonClicked -= LoadMoreImages;
            // galleryView.ScreenshotThumbnailClicked -= ShowScreenshotWithMetadata;

            // if (screenshotViewer != null)
            // {
            //     screenshotViewer.PrevScreenshotClicked -= ShowPrevScreenshot;
            //     screenshotViewer.NextScreenshotClicked -= ShowNextScreenshot;
            // }

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

            if (firstLoad)
                cameraReelModel.LoadImagesAsync();
        }

        //
        // private async Task InstantiateScreenshotViewer()
        // {
        //     screenshotViewer = await assetProvider.Instantiate<ScreenshotViewerHUDView>(SCREENSHOT_VIEW, "ScreenshotViewer");
        //     // screenshotViewer.PrevScreenshotClicked += ShowPrevScreenshot;
        //     // screenshotViewer.NextScreenshotClicked += ShowNextScreenshot;
        // }
        // private async void ShowScreenshotWithMetadata(CameraReelResponse reelResponse)
        // {
        //     if (screenshotViewer == null)
        //         await InstantiateScreenshotViewer();
        //
        //     screenshotViewer.Show(reelResponse);
        // }
        // private void ShowNextScreenshot(CameraReelResponse current)
        // {
        //     CameraReelResponse next = galleryView.GetNextScreenshot(current);
        //
        //     if (next != null)
        //         ShowScreenshotWithMetadata(next);
        // }
        //
        // private void ShowPrevScreenshot(CameraReelResponse current)
        // {
        //     CameraReelResponse prev = galleryView.GetPreviousScreenshot(current);
        //
        //     if (prev != null)
        //         ShowScreenshotWithMetadata(prev);
        // }
    }
}
