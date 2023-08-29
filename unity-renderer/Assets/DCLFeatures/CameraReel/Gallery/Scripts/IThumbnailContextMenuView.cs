using DCLServices.CameraReelService;
using System;

namespace DCLFeatures.CameraReel.Gallery
{
    public interface IThumbnailContextMenuView : IDisposable
    {
        event Action OnDownloadRequested;
        event Action OnDeletePictureRequested;
        event Action OnCopyPictureLinkRequested;
        event Action OnShareToTwitterRequested;
        event Action<CameraReelResponse> OnSetup;
    }
}
