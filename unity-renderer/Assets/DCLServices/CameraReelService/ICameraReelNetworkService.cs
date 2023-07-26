using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;
using UI.InWorldCamera.Scripts;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelNetworkService : IService
    {
        UniTask<CameraReelResponse> GetScreenshot(string uuid, CancellationToken ct = default);

        UniTask<CameraReelResponse> UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct = default);
    }
}
