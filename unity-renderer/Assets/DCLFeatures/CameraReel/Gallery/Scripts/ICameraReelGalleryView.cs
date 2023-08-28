using DCLServices.CameraReelService;
using System;

namespace DCLFeatures.CameraReel.Gallery
{
    public interface ICameraReelGalleryView
    {
        event Action<CameraReelResponse> ScreenshotThumbnailClicked;
        event Action ShowMoreButtonClicked;

        void AddScreenshotThumbnail(CameraReelResponse reel, bool setAsFirst);
        void DeleteScreenshotThumbnail(CameraReelResponse reel);
        void SwitchVisibility(bool isVisible);
        void SwitchEmptyStateVisibility(bool visible);
        void SwitchShowMoreVisibility(bool visible);
    }
}
