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

        public event Action<CameraReelResponses> Updated;

        public bool IsUpdating { get; private set; }

        public async void LoadImagesAsync()
        {
            IsUpdating = true;

            CameraReelResponses reelImages = await cameraReelNetworkService.GetScreenshotGallery(
                DataStore.i.player.ownPlayer.Get().id, LIMIT, offset);

            offset += LIMIT;

            foreach (CameraReelResponse reel in reelImages.images)
                reels.AddLast(reel);

            IsUpdating = false;
            Updated?.Invoke(reelImages);
        }

        public async void RemoveScreenshot(CameraReelResponse current)
        {
            LinkedListNode<CameraReelResponse> nodeToRemove = reels.Find(current);

            if (nodeToRemove != null)
                reels.Remove(nodeToRemove);
        }

        public CameraReelResponse GetNextScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Next?.Value;

        public CameraReelResponse GetPreviousScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Previous?.Value;
    }
}
