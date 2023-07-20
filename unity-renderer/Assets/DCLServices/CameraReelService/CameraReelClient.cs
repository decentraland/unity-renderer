using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using UI.InWorldCamera.Scripts;
using UnityEngine;
using UnityEngine.Networking;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelClient
    {
        UniTask<CameraReelImageResponse> GetImage(string imageUUID, CancellationToken ct);

        UniTask<CameraReelImageResponse> UploadScreenshot(byte[] screenshot, ScreenshotMetadata metadata, CancellationToken ct);

        UniTask GetImageMetadata(string imageUUID, CancellationToken ct);

        UniTask DeleteImage(string imageUUID, CancellationToken ct);
    }

    public class CameraReelClient : ICameraReelClient
    {
        private const string IMAGE_BASE_URL = "https://camera-reel-service.decentraland.zone/api/images";
        private const string USER_BASE_URL = "https://camera-reel-service.decentraland.zone/api/users/me/images";

        private readonly IWebRequestController webRequestController;

        public CameraReelClient(IWebRequestController webRequestController)
        {
            this.webRequestController = webRequestController;
        }

        public async UniTask<CameraReelImageResponse> UploadScreenshot(byte[] screenshot, ScreenshotMetadata metadata, CancellationToken ct)
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("image", screenshot, $"{metadata.dateTime}.jpg", "image/jpeg"),
                new MultipartFormDataSection("metadata", JsonUtility.ToJson(metadata)),
            };

            UnityWebRequest result = await webRequestController.PostAsync(IMAGE_BASE_URL, formData, cancellationToken: ct);
            return ParseScreenshotResponse(result, unSuccessResultMassage: "Error uploading screenshot");
        }

        public async UniTask<CameraReelImageResponse> GetImage(string imageUUID, CancellationToken ct)
        {
            UnityWebRequest result = await webRequestController.GetAsync($"{IMAGE_BASE_URL}/{imageUUID}", cancellationToken: ct);
            return ParseScreenshotResponse(result, unSuccessResultMassage: "Error fetching screenshot image with metadata");
        }

        private static CameraReelImageResponse ParseScreenshotResponse(UnityWebRequest result, string unSuccessResultMassage)
        {
            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"{unSuccessResultMassage}:\n{result.error}");

            CameraReelImageResponse response = Utils.SafeFromJson<CameraReelImageResponse>(result.downloadHandler.text);
            ResponseSanityCheck(response, result.downloadHandler.text);
            return response;
        }

        private static void ResponseSanityCheck(CameraReelImageResponse response, string downloadHandlerText)
        {
            if (response == null)
                throw new Exception($"Error parsing screenshot response:\n{downloadHandlerText}");

            if (string.IsNullOrEmpty(response.url))
                throw new Exception($"No screenshot image url info retrieved:\n{downloadHandlerText}");

            if (response.metadata == null)
                throw new Exception($"No screenshot metadata info retrieved:\n{downloadHandlerText}");
        }

        public UniTask GetImageMetadata(string imageUUID, CancellationToken ct) =>
            throw new NotImplementedException();

        public UniTask DeleteImage(string imageUUID, CancellationToken ct) =>
            throw new NotImplementedException();
    }

    [Serializable]
    public class CameraReelImageResponse
    {
        public string id;
        public string url;
        public string thumbnailUrl;

        public ScreenshotMetadata metadata;
    }
}
