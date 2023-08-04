using Cysharp.Threading.Tasks;
using System.Threading;
using UI.InWorldCamera.Scripts;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelClient
    {
        UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct);

        UniTask<CameraReelUploadResponse> UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct);

        UniTask<CameraReelDeleteResponse> DeleteScreenshot(string uuid, CancellationToken ct);
    }
}
