using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Threading;
using System.Threading.Tasks;
using UI.InWorldCamera.Scripts;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelNetworkService : IService
    {
        UniTask<CameraReelResponse> GetScreenshot(string uuid, CancellationToken ct = default);
        Task<CameraReelResponse[]> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct = default);

        UniTask<CameraReelResponse> UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct = default);
        UniTask DeleteScreenshot(string uuid, CancellationToken ct = default);
    }

    public class CameraReelNetworkService : ICameraReelNetworkService
    {
        private readonly ICameraReelClient client;

        public CameraReelNetworkService(ICameraReelClient client)
        {
            this.client = client;
        }

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<CameraReelResponse> GetScreenshot(string uuid, CancellationToken ct) =>
            await client.GetScreenshot(uuid, ct);

        public async Task<CameraReelResponse[]> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct) =>
            await client.GetScreenshotGallery(userAddress, limit, offset, ct);

        public async UniTask<CameraReelResponse> UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct) =>
            await client.UploadScreenshot(image, metadata, ct);

        public async UniTask DeleteScreenshot(string uuid, CancellationToken ct = default) =>
            await client.DeleteScreenshot(uuid, ct);
    }
}
