using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UI.InWorldCamera.Scripts;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    public class CameraReelService : IScreenshotCameraService, ICameraReelGalleryService
    {
        private readonly ICameraReelClient client;

        private IScreenshotCamera screenshotCamera;
        public event Action<CameraReelResponse> ScreenshotUploaded;

        public CameraReelService(ICameraReelClient client)
        {
            this.client = client;
        }

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct) =>
            await client.GetScreenshotGallery(userAddress, limit, offset, ct);

        public async UniTask DeleteScreenshot(string uuid, CancellationToken ct = default) =>
            await client.DeleteScreenshot(uuid, ct);

        public async UniTask UploadScreenshot(Texture2D texture, ScreenshotMetadata metadata, CancellationToken ct)
        {
            var response = await client.UploadScreenshot(texture.EncodeToJPG(), metadata, ct);
            ScreenshotUploaded?.Invoke(response);
        }

        public void SetCamera(IScreenshotCamera screenshotCamera) =>
            this.screenshotCamera = screenshotCamera;

        public void EnableScreenshotCamera() =>
            screenshotCamera?.ToggleVisibility(isVisible: true);
    }
}
