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

        public bool IsUpdating { get; private set; }

        private ICameraReelNetworkService cameraReelNetworkService => cameraReelNetworkServiceLazy ??= Environment.i.serviceLocator.Get<ICameraReelNetworkService>();

        public event Action<CameraReelResponses> ScreenshotBatchFetched;
        public event Action<CameraReelResponse> ScreenshotRemovalStarted;
        public event Action<CameraReelResponse> ScreenshotRemovalFailed;
        public event Action<CameraReelResponse> ScreenshotRemovalFinished;

        public async void RequestScreenshotsBatchAsync()
        {
            IsUpdating = true;

            CameraReelResponses reelImages = await cameraReelNetworkService.GetScreenshotGallery(
                DataStore.i.player.ownPlayer.Get().id, LIMIT, offset);

            offset += LIMIT;

            foreach (CameraReelResponse reel in reelImages.images)
                reels.AddLast(reel);

            IsUpdating = false;
            ScreenshotBatchFetched?.Invoke(reelImages);
        }

        public async void RemoveScreenshot(CameraReelResponse current)
        {
            LinkedListNode<CameraReelResponse> nodeToRemove = reels.Find(current);

            if (nodeToRemove != null)
                reels.Remove(nodeToRemove);

            ScreenshotRemovalStarted?.Invoke(current);
            try { await cameraReelNetworkService.DeleteScreenshot(current.id); }
            catch (Exception) { ScreenshotRemovalFailed?.Invoke(current); }
            finally { ScreenshotRemovalFinished?.Invoke(current); }
        }

        public CameraReelResponse GetNextScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Next?.Value;

        public CameraReelResponse GetPreviousScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Previous?.Value;
    }
}
