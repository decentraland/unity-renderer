using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Threading;
using UI.InWorldCamera.Scripts;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    public class CameraReelService : IScreenshotCameraService, ICameraReelGalleryService
    {
        private const string UPLOADING_ERROR_MESSAGE = "There was an unexpected error when uploading the picture. Try again later.";
        private const string STORAGE_LIMIT_REACHED_MESSAGE = "You can't take more pictures because you have reached the storage limit of the camera reel.\nTo make room we recommend you to download your photos and then delete them.";

        private readonly ICameraReelClient client;

        private IScreencaptureCamera screencaptureCamera;
        public event Action<CameraReelResponse> ScreenshotUploaded;

        public CameraReelService(ICameraReelClient client)
        {
            this.client = client;
        }

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct) =>
            await client.GetScreenshotGallery(userAddress, limit, offset, ct);

        public async UniTask DeleteScreenshot(string uuid, CancellationToken ct = default) =>
            await client.DeleteScreenshot(uuid, ct);

        public async UniTask UploadScreenshot(Texture2D texture, ScreenshotMetadata metadata, CancellationToken ct)
        {
            CameraReelResponse response = null;

            try { response = await client.UploadScreenshot(texture.EncodeToJPG(), metadata, ct); }
            catch (OperationCanceledException) { }
            catch (ScreenshotLimitReachedException) { DataStore.i.notifications.DefaultErrorNotification.Set(STORAGE_LIMIT_REACHED_MESSAGE, true); }
            catch (Exception) { DataStore.i.notifications.DefaultErrorNotification.Set(UPLOADING_ERROR_MESSAGE, true); }
            finally { ScreenshotUploaded?.Invoke(response); }
        }

        public void SetCamera(IScreencaptureCamera screencaptureCamera) =>
            this.screencaptureCamera = screencaptureCamera;

        public void EnableScreenshotCamera() =>
            screencaptureCamera?.ToggleVisibility(isVisible: true);
    }
}
