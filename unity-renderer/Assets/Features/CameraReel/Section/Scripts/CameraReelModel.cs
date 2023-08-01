using Cysharp.Threading.Tasks;
using DCL;
using DCLServices.CameraReelService;
using System;
using System.Collections.Generic;
using UI.InWorldCamera.Scripts;
using UnityEngine;
using Environment = DCL.Environment;

namespace Features.CameraReel.Section
{
    public class CameraReelModel
    {
        private const int LIMIT = 20;
        private readonly LinkedList<CameraReelResponse> reels = new ();

        private ICameraReelGalleryService cameraReelServiceLazy;
        private int offset;

        public bool IsUpdating { get; private set; }

        private ICameraReelGalleryService cameraReelService => cameraReelServiceLazy ??= Environment.i.serviceLocator.Get<ICameraReelGalleryService>();

        public event Action<CameraReelResponses> ScreenshotBatchFetched;
        public event Action<CameraReelResponse, UniTask> ScreenshotRemovalStarted;
        public event Action<CameraReelResponse> ScreenshotUploaded;

        public CameraReelModel()
        {
            cameraReelService.ScreenshotUploadStarted += OnScreenshotUploaded;
        }

        private async void OnScreenshotUploaded(Texture2D screenshotImage, ScreenshotMetadata metadata, UniTask<CameraReelResponse> webRequest)
        {
            // TODO: Handle dummy image in the gallery while awaiting for the real one
            // ScreenshotUploadStarted?.Invoke(screenshotImage, metadata);

            CameraReelResponse response = await webRequest;
            reels.AddFirst(response);
            ScreenshotUploaded?.Invoke(response);
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

            UniTask request = cameraReelService.DeleteScreenshot(current.id);
            ScreenshotRemovalStarted?.Invoke(current, request);
            await request;
        }

        public CameraReelResponse GetNextScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Next?.Value;

        public CameraReelResponse GetPreviousScreenshot(CameraReelResponse current) =>
            reels.Find(current)?.Previous?.Value;
    }
}
