using Cysharp.Threading.Tasks;
using System.Threading;
using UI.InWorldCamera.Scripts;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelClient
    {
        UniTask<CameraReelResponse> GetScreenshot(string uuid, CancellationToken ct);

        UniTask<CameraReelResponse> UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct);
    }
}
