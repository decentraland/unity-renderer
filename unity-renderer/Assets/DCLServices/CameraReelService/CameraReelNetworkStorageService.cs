using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    public class CameraReelNetworkStorageService : ICameraReelStorageService
    {
        private readonly ICameraReelNetworkClient networkClient;

        private ICameraReelAnalyticsService analyticsServiceLazy;

        public CameraReelNetworkStorageService(ICameraReelNetworkClient networkClient)
        {
            this.networkClient = networkClient;
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
            return (response.image, new CameraReelStorageStatus(response.currentImages, response.maxImages));
        }
    }
}
