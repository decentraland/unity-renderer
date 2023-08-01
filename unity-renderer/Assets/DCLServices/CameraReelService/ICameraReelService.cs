using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Threading;
using UI.InWorldCamera.Scripts;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelService : IService, IScreenshotCameraService, ICameraReelGalleryService { }

    public interface ICameraReelGalleryService
    {
        event Action<byte[], ScreenshotMetadata, UniTask<CameraReelResponse>> ScreenshotUploadStarted;

        UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct = default);

        UniTask DeleteScreenshot(string uuid, CancellationToken ct = default);

        void EnableScreenshotCamera();
    }

    public interface IScreenshotCameraService
    {
        UniTask UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct = default);

        void SetCamera(IScreenshotCamera screenshotCamera);
    }
}
