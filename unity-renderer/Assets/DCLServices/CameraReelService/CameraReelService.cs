using Cysharp.Threading.Tasks;
using System.Threading;
using UI.InWorldCamera.Scripts;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    public class CameraReelService : ICameraReelService
    {
        private readonly ICameraReelClient client;
        private readonly IUserProfileBridge userProfileBridge;

        private IScreencaptureCamera screencaptureCamera;

        public CameraReelService(ICameraReelClient client,
            IUserProfileBridge userProfileBridge)
        {
            this.client = client;
            this.userProfileBridge = userProfileBridge;
        }

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct) =>
            await client.GetScreenshotGallery(userAddress, limit, offset, ct);

        public async UniTask<CameraReelStorageStatus> DeleteScreenshot(string uuid, CancellationToken ct = default)
        {
            await client.DeleteScreenshot(uuid, ct);
            CameraReelResponses reel = await GetScreenshotGallery(userProfileBridge.GetOwn().userId, 0, 0, ct);
            return new CameraReelStorageStatus(reel.currentImages, reel.maxImages);
        }

        public async UniTask<(CameraReelResponse, CameraReelStorageStatus)> UploadScreenshot(Texture2D texture, ScreenshotMetadata metadata, CancellationToken ct)
        {
            CameraReelUploadResponse response = await client.UploadScreenshot(texture.EncodeToJPG(), metadata, ct);
            CameraReelStorageStatus storage = new CameraReelStorageStatus(response.currentImages, response.maxImages);

            return (response.image, storage);
        }

        public void SetCamera(IScreencaptureCamera screencaptureCamera) =>
            this.screencaptureCamera = screencaptureCamera;

        public void EnableScreenshotCamera() =>
            screencaptureCamera?.ToggleScreenshotCamera(isEnabled: true);
    }
}
