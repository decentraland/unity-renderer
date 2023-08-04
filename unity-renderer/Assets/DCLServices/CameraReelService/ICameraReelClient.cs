using Cysharp.Threading.Tasks;
using System.Threading;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelClient
    {
        UniTask<CameraReelStorageResponse> GetUserGalleryStorageInfo(string userAddress, CancellationToken ct);

        UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct);

        UniTask<CameraReelUploadResponse> UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct);

        UniTask<CameraReelStorageResponse> DeleteScreenshot(string uuid, CancellationToken ct);
    }
}
