using Cysharp.Threading.Tasks;
using DCL;
using System.Threading;
using UI.InWorldCamera.Scripts;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelService : IService
    {
        UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct = default);

        UniTask<CameraReelStorageStatus> DeleteScreenshot(string uuid, CancellationToken ct = default);

        UniTask<(CameraReelResponse, CameraReelStorageStatus)> UploadScreenshot(Texture2D image, ScreenshotMetadata metadata, CancellationToken ct = default);

        void SetCamera(IScreencaptureCamera screencaptureCamera);

        void EnableScreenshotCamera();
    }
}
