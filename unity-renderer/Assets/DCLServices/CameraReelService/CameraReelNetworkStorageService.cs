using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    public class CameraReelNetworkStorageService : ICameraReelStorageService
    {
        private readonly ICameraReelNetworkClient networkClient;
        private readonly ServiceLocator serviceLocator;

        private ICameraReelAnalyticsService analyticsServiceLazy;
        private ICameraReelAnalyticsService analyticsService => analyticsServiceLazy ??= serviceLocator.Get<ICameraReelAnalyticsService>();

        public CameraReelNetworkStorageService(ICameraReelNetworkClient networkClient, ServiceLocator serviceLocator)
        {
            this.networkClient = networkClient;
            this.serviceLocator = serviceLocator;
        }

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<CameraReelStorageStatus> GetUserGalleryStorageInfo(string userAddress, CancellationToken ct = default)
        {
            CameraReelStorageResponse response = await networkClient.GetUserGalleryStorageInfoRequest(userAddress, ct);
            return new CameraReelStorageStatus(response.currentImages, response.maxImages);
        }

        public async UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct) =>
            await networkClient.GetScreenshotGalleryRequest(userAddress, limit, offset, ct);

        public async UniTask<CameraReelStorageStatus> DeleteScreenshot(string uuid, CancellationToken ct = default)
        {
            CameraReelStorageResponse response = await networkClient.DeleteScreenshotRequest(uuid, ct);
            return new CameraReelStorageStatus(response.currentImages, response.maxImages);
        }

        public async UniTask<(CameraReelResponse, CameraReelStorageStatus)> UploadScreenshot(Texture2D image, ScreenshotMetadata metadata, CancellationToken ct = default)
        {
            CameraReelUploadResponse response = await networkClient.UploadScreenshotRequest(image.EncodeToJPG(), metadata, ct);
            analyticsService.SendScreenshotUploaded(metadata.userAddress, metadata.realm, metadata.scene.name, metadata.GetLocalizedDateTime().ToString("MMMM dd, yyyy"), metadata.visiblePeople.Length);
            return (response.image, new CameraReelStorageStatus(response.currentImages, response.maxImages));
        }
    }
}
