﻿using Cysharp.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using UI.InWorldCamera.Scripts;

namespace DCLServices.CameraReelService
{
    public class CameraReelNetworkService : ICameraReelNetworkService
    {
        private readonly ICameraReelClient client;

        public CameraReelNetworkService(ICameraReelClient client)
        {
            this.client = client;
        }

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct) =>
            await client.GetScreenshotGallery(userAddress, limit, offset, ct);

        public async UniTask<CameraReelResponse> UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct) =>
            await client.UploadScreenshot(image, metadata, ct);

        public async UniTask DeleteScreenshot(string uuid, CancellationToken ct = default) =>
            await client.DeleteScreenshot(uuid, ct);
    }
}
