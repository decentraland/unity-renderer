using Cysharp.Threading.Tasks;
using System.Threading;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelNetworkClient
    {
        UniTask<CameraReelStorageResponse> GetUserGalleryStorageInfoRequest(string userAddress, CancellationToken ct);

        UniTask<CameraReelResponses> GetScreenshotGalleryRequest(string userAddress, int limit, int offset, CancellationToken ct);

        UniTask<CameraReelUploadResponse> UploadScreenshotRequest(byte[] image, ScreenshotMetadata metadata, CancellationToken ct);

        UniTask<CameraReelStorageResponse> DeleteScreenshotRequest(string uuid, CancellationToken ct);
    }
}
