using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;
using UI.InWorldCamera.Scripts;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelNetworkService : IService
    {
        UniTask<(string url, ScreenshotMetadata metadata)> GetImage(string imageUUID, CancellationToken ct = default);

        UniTask<CameraReelImageResponse> UploadScreenshot(byte[] screenshot, ScreenshotMetadata metadata, CancellationToken ct = default);
    }

    public class CameraReelNetworkService : ICameraReelNetworkService
    {
        private readonly ICameraReelClient client;

        public CameraReelNetworkService(ICameraReelClient client)
        {
            this.client = client;
        }

        public void Initialize() { }

        public async UniTask<(string, ScreenshotMetadata)> GetImage(string imageUUID, CancellationToken ct) =>
            await client.GetImage(imageUUID, ct);

        public async UniTask<CameraReelImageResponse> UploadScreenshot(byte[] screenshot, ScreenshotMetadata metadata, CancellationToken ct) =>
            await client.UploadScreenshot(screenshot, metadata, ct);

        public void Dispose() { }

    }
}
