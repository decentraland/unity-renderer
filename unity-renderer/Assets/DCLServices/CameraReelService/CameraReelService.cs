using Cysharp.Threading.Tasks;
using DCL;
using System;
using System.Threading;
using UI.InWorldCamera.Scripts;
using UnityEngine;

namespace DCLServices.CameraReelService
{
    public class CameraReelService : ICameraReelService
    {
        private const string UPLOADING_ERROR_MESSAGE = "There was an unexpected error when uploading the picture. Try again later.";
        private const string STORAGE_LIMIT_REACHED_MESSAGE = "You can't take more pictures because you have reached the storage limit of the camera reel.\nTo make room we recommend you to download your photos and then delete them.";

        private readonly ICameraReelClient client;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;

        private IScreencaptureCamera screencaptureCamera;

        public CameraReelService(ICameraReelClient client,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge)
        {
            this.client = client;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
        }

        public void Initialize() { }

        public void Dispose() { }

        public async UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct) =>
            await client.GetScreenshotGallery(userAddress, limit, offset, ct);

        public async UniTask<CameraReelStorageStatus> DeleteScreenshot(string uuid, CancellationToken ct = default)
        {
            await client.DeleteScreenshot(uuid, ct);
            CameraReelResponses reel = await GetScreenshotGallery(userProfileBridge.GetOwn().userId, 0, 0, ct);
            return new CameraReelStorageStatus(reel.currentImages, reel.maxImages);
        }

        public async UniTask<(CameraReelResponse, CameraReelStorageStatus)> UploadScreenshot(Texture2D texture, ScreenshotMetadata metadata, CancellationToken ct)
        {
            CameraReelResponse response = null;
            CameraReelStorageStatus storage = null;

            try
            {
                response = await client.UploadScreenshot(texture.EncodeToJPG(), metadata, ct);
                CameraReelResponses reel = await GetScreenshotGallery(userProfileBridge.GetOwn().userId, 0, 0, ct);
                storage = new CameraReelStorageStatus(reel.currentImages, reel.maxImages);
            }
            catch (OperationCanceledException) { }
            catch (ScreenshotLimitReachedException) { dataStore.notifications.DefaultErrorNotification.Set(STORAGE_LIMIT_REACHED_MESSAGE, true); }
            catch (Exception) { dataStore.notifications.DefaultErrorNotification.Set(UPLOADING_ERROR_MESSAGE, true); }
            return (response, storage);
        }

        public void SetCamera(IScreencaptureCamera screencaptureCamera) =>
            this.screencaptureCamera = screencaptureCamera;

        public void EnableScreenshotCamera() =>
            screencaptureCamera?.ToggleScreenshotCamera(isEnabled: true);
    }
}
