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
        UniTask<(string, ScreenshotMetadata)> GetImage(string imageUUID, CancellationToken ct);

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
            Debug.Log("START UPLOAD");

            List<IMultipartFormSection> formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("image", screenshot, $"{metadata.dateTime}.jpg", "image/jpeg"),
                new MultipartFormDataSection("metadata", JsonUtility.ToJson(metadata)),
            };

            var result = await webRequestController.PostMultipartAsync(IMAGE_BASE_URL, formData, cancellationToken: ct);

            Debug.Log("UPLOAD RESULT");

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error uploading screenshot:\n{result.error}");
            Debug.Log("ORIGINAL:" + result.downloadHandler.text);

            string trimmedResponse = result.downloadHandler.text.Substring(9, result.downloadHandler.text.Length - 2);
            Debug.Log("TRIMMED:" + trimmedResponse);

            var response = Utils.SafeFromJson<CameraReelImageResponse>(trimmedResponse);
            ResponseSanityCheck(response, result.downloadHandler.text);
            Debug.Log("UPLOAD SANITY CHECK PASSED");

            return response;
        }

        public async UniTask<(string, ScreenshotMetadata)> GetImage(string imageUUID, CancellationToken ct)
        {
            var url = $"{IMAGE_BASE_URL}/{imageUUID}";
            var result = await webRequestController.GetAsync(url, cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching screenshot image with metadata:\n{result.error}");

            Debug.Log(result.downloadHandler.text);

            var response = Utils.SafeFromJson<CameraReelImageResponse>(result.downloadHandler.text);
            ResponseSanityCheck(response, result.downloadHandler.text);

            return (response.url, response.metadata);
        }

        private static void ResponseSanityCheck(CameraReelImageResponse response, string downloadHandlerText)
        {
            if (response == null)
                throw new Exception($"Error parsing screenshot response:\n{downloadHandlerText}");

            if (string.IsNullOrEmpty(response.url))
                throw new Exception($"No image url info retrieved:\n{downloadHandlerText}");

            if (response.metadata == null)
                throw new Exception($"No metadata info retrieved:\n{downloadHandlerText}");
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
        public ScreenshotMetadata metadata;
    }

}
