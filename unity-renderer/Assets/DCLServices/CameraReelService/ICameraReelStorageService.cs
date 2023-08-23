using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Threading;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelStorageService : IService
    {
        event Action<CameraReelResponse, CameraReelStorageStatus> ScreenshotUploaded;

        UniTask<CameraReelStorageStatus> GetUserGalleryStorageInfo(string userAddress, CancellationToken ct = default);

        UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct = default);

        UniTask<CameraReelStorageStatus> DeleteScreenshot(string uuid, CancellationToken ct = default);

        UniTask<CameraReelStorageStatus> UploadScreenshot(Texture2D image, ScreenshotMetadata metadata, CancellationToken ct = default);
    }
}
