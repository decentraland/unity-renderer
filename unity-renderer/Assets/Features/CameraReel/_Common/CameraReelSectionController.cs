using CameraReel.Gallery;
using System;

namespace Features.CameraReel
{
    public class CameraReelSectionController: IDisposable
    {
        private readonly CameraReelSectionView sectionView;
        private readonly CameraReelGalleryView galleryView;
        private readonly CameraReelGalleryStorageView galleryStorageView;

        public CameraReelSectionController(CameraReelSectionView sectionView, CameraReelGalleryView galleryView, CameraReelGalleryStorageView galleryStorageView)
        {
            this.sectionView = sectionView;
            this.galleryView = galleryView;
            this.galleryStorageView = galleryStorageView;
        }

        public void Initialize()
        {
            galleryView.ScreenshotsStorageUpdated += UpdateStorageBar;
        }

        public void Dispose()
        {
            galleryView.ScreenshotsStorageUpdated -= UpdateStorageBar;
        }

        private void UpdateStorageBar((int current, int max) storage) =>
            galleryStorageView.UpdateStorageBar(storage.current, storage.max);
    }
}
