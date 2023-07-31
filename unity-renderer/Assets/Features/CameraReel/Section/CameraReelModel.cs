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

        private ICameraReelService cameraReelServiceLazy;
        private int offset;

        public bool IsUpdating { get; private set; }

        private ICameraReelService cameraReelService => cameraReelServiceLazy ??= Environment.i.serviceLocator.Get<ICameraReelService>();

        public event Action<CameraReelResponses> ScreenshotBatchFetched;
        public event Action<CameraReelResponse> ScreenshotRemovalStarted;
        public event Action<CameraReelResponse> ScreenshotUploaded;


        public CameraReelModel()
        {
            cameraReelService.ScreenshotUploaded += OnScreenshotUploaded;
        }

        private void OnScreenshotUploaded(CameraReelResponse screenshot)
        {
            reels.AddFirst(screenshot);
            ScreenshotUploaded?.Invoke(screenshot);
        }

        public async void RequestScreenshotsBatchAsync()
        {
            IsUpdating = true;

            CameraReelResponses reelImages = await cameraReelService.GetScreenshotGallery(
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
            await cameraReelService.DeleteScreenshot(current.id);
            // try { await cameraReelService.DeleteScreenshot(current.id); }
            // catch (Exception) { ScreenshotRemovalFailed?.Invoke(current); }
            // finally { ScreenshotRemovalFinished?.Invoke(current); }
        }

        public CameraReelResponse GetNextScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Next?.Value;

        public CameraReelResponse GetPreviousScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Previous?.Value;
    }
}
