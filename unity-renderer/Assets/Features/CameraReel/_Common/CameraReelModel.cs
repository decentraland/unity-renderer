using DCL;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using Environment = DCL.Environment;

namespace Features.CameraReel
{
    public class CameraReelModel
    {
        private const int LIMIT = 20;
        private readonly LinkedList<CameraReelResponse> reels = new ();
        private ICameraReelNetworkService cameraReelNetworkServiceLazy;
        private int offset;
        private ICameraReelNetworkService cameraReelNetworkService => cameraReelNetworkServiceLazy ??= Environment.i.serviceLocator.Get<ICameraReelNetworkService>();

        public event Action<CameraReelResponses> LoadedScreenshotsUpdated;

        public async void LoadImagesAsync()
        {
            CameraReelResponses reelImages = await cameraReelNetworkService.GetScreenshotGallery(
                DataStore.i.player.ownPlayer.Get().id, LIMIT, offset);

            offset += LIMIT;

            foreach (CameraReelResponse reel in reelImages.images)
                reels.AddLast(reel);

            LoadedScreenshotsUpdated?.Invoke(reelImages);
        }
    }
}
