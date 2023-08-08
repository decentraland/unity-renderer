using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    public class CameraReelService : ICameraReelService
    {
        private readonly ICameraReelClient client;
        private readonly ServiceLocator serviceLocator;

        private IScreencaptureCamera screencaptureCamera;

        private ICameraReelAnalyticsService analyticsServiceLazy;
        private ICameraReelAnalyticsService analyticsService => analyticsServiceLazy ??= serviceLocator.Get<ICameraReelAnalyticsService>();

        public CameraReelService(ICameraReelClient client, ServiceLocator serviceLocator)
        {
            this.client = client;
            this.serviceLocator = serviceLocator;
        }

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<CameraReelStorageStatus> GetUserGalleryStorageInfo(string userAddress, CancellationToken ct = default)
        {
            CameraReelStorageResponse response = await client.GetUserGalleryStorageInfo(userAddress, ct);
            return new CameraReelStorageStatus(response.currentImages, response.maxImages);
        }

        public async UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct) =>
            await client.GetScreenshotGallery(userAddress, limit, offset, ct);

        public async UniTask<CameraReelStorageStatus> DeleteScreenshot(string uuid, CancellationToken ct = default)
        {
            CameraReelStorageResponse response = await client.DeleteScreenshot(uuid, ct);
            return new CameraReelStorageStatus(response.currentImages, response.maxImages);
        }

        public async UniTask<(CameraReelResponse, CameraReelStorageStatus)> UploadScreenshot(Texture2D image, ScreenshotMetadata metadata, CancellationToken ct = default)
        {
            CameraReelUploadResponse response = await client.UploadScreenshot(image.EncodeToJPG(), metadata, ct);
            analyticsService.SendScreenshotUploaded(metadata.userAddress, metadata.realm, metadata.scene.name, metadata.GetLocalizedDateTime().ToString("MMMM dd, yyyy"), metadata.visiblePeople.Length);
            return (response.image, new CameraReelStorageStatus(response.currentImages, response.maxImages));
        }

        public void SetCamera(IScreencaptureCamera screencaptureCamera) =>
            this.screencaptureCamera = screencaptureCamera;

        public void EnableScreenshotCamera() =>
            screencaptureCamera?.ToggleScreenshotCamera(isEnabled: true);
    }
}
