using Cysharp.Threading.Tasks;
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
        private readonly ICameraReelGalleryService galleryService;
        private readonly Func<ScreenshotViewerController> screenshotViewerControllerFactory;

        private ScreenshotViewerController screenshotViewerController;
        private bool isUpdating;
        private bool firstLoad = true;
        private int offset;
        private CancellationTokenSource fetchScreenshotsCancellationToken;

        public CameraReelSectionController(CameraReelSectionView sectionView,
            CameraReelGalleryView galleryView,
            CameraReelGalleryStorageView galleryStorageView,
            DataStore dataStore,
            ICameraReelGalleryService galleryService,
            CameraReelModel cameraReelModel,
            Func<ScreenshotViewerController> screenshotViewerControllerFactory)
        {
            this.sectionView = sectionView;

            this.galleryStorageView = galleryStorageView;
            this.dataStore = dataStore;
            this.galleryService = galleryService;
            this.screenshotViewerControllerFactory = screenshotViewerControllerFactory;
            this.galleryView = galleryView;

            this.cameraReelModel = cameraReelModel;
            this.cameraReelModel.ScreenshotRemoved += galleryView.DeleteScreenshotThumbnail;
            this.cameraReelModel.ScreenshotAdded += galleryView.AddScreenshotThumbnail;

            galleryService.ScreenshotUploaded += this.cameraReelModel.AddScreenshotAsFirst;

            dataStore.HUDs.cameraReelSectionVisible.OnChange += SwitchGalleryVisibility;

            galleryView.ShowMoreButtonClicked += FetchScreenshotsAndUpdateStorageStatus;
            galleryView.ScreenshotThumbnailClicked += ShowScreenshotWithMetadata;
        }

        public void Dispose()
        {
            cameraReelModel.ScreenshotRemoved -= galleryView.DeleteScreenshotThumbnail;
            cameraReelModel.ScreenshotAdded -= galleryView.AddScreenshotThumbnail;

            dataStore.HUDs.cameraReelSectionVisible.OnChange -= SwitchGalleryVisibility;
            galleryView.ShowMoreButtonClicked -= FetchScreenshotsAndUpdateStorageStatus;
            galleryView.ScreenshotThumbnailClicked -= ShowScreenshotWithMetadata;

            galleryService.ScreenshotUploaded -= cameraReelModel.AddScreenshotAsFirst;
        }

        private void SwitchGalleryVisibility(bool isVisible, bool _)
        {
            sectionView.SwitchVisibility(isVisible);

            if (firstLoad && !isUpdating)
                FetchScreenshotsAndUpdateStorageStatus();
        }

        private void ShowScreenshotWithMetadata(CameraReelResponse reelResponse)
        {
            screenshotViewerController ??= screenshotViewerControllerFactory.Invoke();
            screenshotViewerController.Show(reelResponse);
        }

        private void FetchScreenshotsAndUpdateStorageStatus()
        {
            async UniTaskVoid FetchScreenshotsAndUpdateUiAsync(CancellationToken cancellationToken)
            {
                CameraReelResponses reelImages = await FetchScreenshotsAsync(cancellationToken);

                galleryStorageView.UpdateStorageBar(reelImages.currentImages, reelImages.maxImages);

                if (firstLoad)
                {
                    sectionView.ShowGalleryWhenLoaded();
                    firstLoad = false;
                }
            }

            fetchScreenshotsCancellationToken = fetchScreenshotsCancellationToken.SafeRestart();
            FetchScreenshotsAndUpdateUiAsync(fetchScreenshotsCancellationToken.Token).Forget();
        }

        private async UniTask<CameraReelResponses> FetchScreenshotsAsync(CancellationToken cancellationToken)
        {
            isUpdating = true;

            CameraReelResponses reelImages = await galleryService.GetScreenshotGallery(
                dataStore.player.ownPlayer.Get().id, LIMIT, offset, cancellationToken);

            offset += LIMIT;

            foreach (CameraReelResponse reel in reelImages.images)
                cameraReelModel.AddScreenshotAsLast(reel);

            isUpdating = false;

            return reelImages;
        }
    }
}
