using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Threading;
using UI.InWorldCamera.Scripts;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelService : IService
    {
        event Action<byte[], ScreenshotMetadata> ScreenshotUploadStarted;
        event Action<string> ScreenshotUploadFailed;
        event Action<CameraReelResponse> ScreenshotUploaded;

        UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct = default);
        UniTask UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct = default);
        UniTask DeleteScreenshot(string uuid, CancellationToken ct = default);
    }
}
