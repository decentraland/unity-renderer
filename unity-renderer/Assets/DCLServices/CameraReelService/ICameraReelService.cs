using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;
using UI.InWorldCamera.Scripts;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelGalleryService : IService
    {
        UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct = default);

        UniTask<CameraReelStorageStatus> DeleteScreenshot(string uuid, CancellationToken ct = default);

        void EnableScreenshotCamera();
    }

    public interface IScreenshotCameraService : IService
    {
        UniTask<(CameraReelResponse, CameraReelStorageStatus)> UploadScreenshot(Texture2D image, ScreenshotMetadata metadata, CancellationToken ct = default);

        void SetCamera(IScreencaptureCamera screencaptureCamera);
    }
}
