using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UI.InWorldCamera.Scripts;

namespace DCLServices.CameraReelService
{
    public class CameraReelService : ICameraReelService
    {
        private readonly ICameraReelClient client;

        private IScreenshotCamera screenshotCamera;
        public event Action<byte[], ScreenshotMetadata, UniTask<CameraReelResponse>> ScreenshotUploadStarted;

        public CameraReelService(ICameraReelClient client)
        {
            this.client = client;
        }

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct) =>
            await client.GetScreenshotGallery(userAddress, limit, offset, ct);

        public async UniTask UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct)
        {
            UniTask<CameraReelResponse> request = client.UploadScreenshot(image, metadata, ct);

            ScreenshotUploadStarted?.Invoke(image, metadata, request);

            await client.UploadScreenshot(image, metadata, ct);
        }

        public async UniTask DeleteScreenshot(string uuid, CancellationToken ct = default) =>
            await client.DeleteScreenshot(uuid, ct);

        public void SetCamera(IScreenshotCamera screenshotCamera) =>
            this.screenshotCamera = screenshotCamera;

        public void EnableScreenshotCamera() =>
            screenshotCamera?.ToggleVisibility(isVisible: true);
    }
}
