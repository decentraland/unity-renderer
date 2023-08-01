using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Threading;
using UI.InWorldCamera.Scripts;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelGalleryService : IService
    {
        event Action<CameraReelResponse> ScreenshotUploaded;

        UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct = default);

        UniTask DeleteScreenshot(string uuid, CancellationToken ct = default);

        void EnableScreenshotCamera();
    }

    public interface IScreenshotCameraService : IService
    {
        UniTask UploadScreenshot(Texture2D image, ScreenshotMetadata metadata, CancellationToken ct = default);

        void SetCamera(IScreenshotCamera screenshotCamera);
    }
}
