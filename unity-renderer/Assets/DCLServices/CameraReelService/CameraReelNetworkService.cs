using Cysharp.Threading.Tasks;
using System.Threading;
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

        public async UniTask<CameraReelResponse> GetScreenshot(string uuid, CancellationToken ct) =>
            await client.GetScreenshot(uuid, ct);

        public async UniTask<CameraReelResponse> UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct) =>
            await client.UploadScreenshot(image, metadata, ct);
    }
}
