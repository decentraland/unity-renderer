using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UI.InWorldCamera.Scripts;

namespace DCLServices.CameraReelService
{
    public class CameraReelService : ICameraReelService
    {
        public event Action<byte[], ScreenshotMetadata> ScreenshotUploadStarted;
        public event Action<string> ScreenshotUploadFailed;
        public event Action<CameraReelResponse> ScreenshotUploaded;

        private readonly ICameraReelClient client;

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
            ScreenshotUploadStarted?.Invoke(image, metadata);

            CameraReelResponse response = null;

            try { response = await client.UploadScreenshot(image, metadata, ct); }
            catch (Exception e)
            {
                ScreenshotUploadFailed?.Invoke(e.Message);
            }
            finally
            {
                ScreenshotUploaded?.Invoke(response);
            }
        }

        public async UniTask DeleteScreenshot(string uuid, CancellationToken ct = default) =>
            await client.DeleteScreenshot(uuid, ct);
    }
}
