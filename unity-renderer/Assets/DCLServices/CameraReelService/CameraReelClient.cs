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
    public class CameraReelClient : ICameraReelClient
    {
        private const string IMAGE_BASE_URL = "https://camera-reel-service.decentraland.zone/api/images";
        private const string GALLERY_BASE_URL = "https://camera-reel-service.decentraland.zone/api/users";

        private readonly IWebRequestController webRequestController;

        public CameraReelClient(IWebRequestController webRequestController)
        {
            this.webRequestController = webRequestController;
        }

        public async UniTask<CameraReelResponses> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct)
        {
            UnityWebRequest result = await webRequestController.GetAsync($"{GALLERY_BASE_URL}/{userAddress}/images?limit={limit}&offset={offset}", cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching user screenshots gallery :\n{result.error}");

            CameraReelResponses responseData = Utils.SafeFromJson<CameraReelResponses>(result.downloadHandler.text);

            if (responseData == null)
                throw new Exception($"Error parsing screenshots gallery response:\n{result.downloadHandler.text}");

            foreach (CameraReelResponse response in responseData.images)
                ResponseSanityCheck(response, result.downloadHandler.text);

            return responseData;
        }

        public async UniTask<CameraReelUploadResponse> UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct)
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("image", image, $"{metadata.dateTime}.jpg", "image/jpeg"),
                new MultipartFormDataSection("metadata", JsonUtility.ToJson(metadata)),
            };

            UnityWebRequest result = await webRequestController.PostAsync(IMAGE_BASE_URL, formData, isSigned: true, cancellationToken: ct);

            CameraReelUploadResponse response = Utils.SafeFromJson<CameraReelUploadResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing screenshot response:\n{result.downloadHandler.text}");

            ResponseSanityCheck(response.image, result.downloadHandler.text);

            return response;
        }

        public async UniTask<CameraReelDeleteResponse> DeleteScreenshot(string uuid, CancellationToken ct)
        {
            UnityWebRequest result = await webRequestController.DeleteAsync($"{IMAGE_BASE_URL}/{uuid}", isSigned: true, cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"error during deleting screenshot from the gallery:\n{result.error}");

            CameraReelDeleteResponse response = Utils.SafeFromJson<CameraReelDeleteResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing screenshot delete response:\n{result.downloadHandler.text}");

            return response;
        }

        private static void ResponseSanityCheck(CameraReelResponse response, string downloadHandlerText)
        {
            if (response == null)
                throw new Exception($"Error parsing screenshot response:\n{downloadHandlerText}");

            if (string.IsNullOrEmpty(response.url))
                throw new Exception($"No screenshot image url info retrieved:\n{downloadHandlerText}");

            if (string.IsNullOrEmpty(response.thumbnailUrl))
                throw new Exception($"No screenshot thumbnail url info retrieved:\n{downloadHandlerText}");

            if (response.metadata == null)
                throw new Exception($"No screenshot metadata info retrieved:\n{downloadHandlerText}");
        }
    }

    [Serializable]
    public class CameraReelResponses
    {
        public List<CameraReelResponse> images = new ();
        public int currentImages;
        public int maxImages;
    }

    [Serializable]
    public class CameraReelResponse
    {
        public string id;
        public string url;
        public string thumbnailUrl;

        public ScreenshotMetadata metadata;
    }

    [Serializable]
    public class CameraReelUploadResponse
    {
        public int currentImages;
        public int maxImages;

        public CameraReelResponse image;
    }

    [Serializable]
    public class CameraReelDeleteResponse
    {
        public int currentImages;
        public int maxImages;
    }

    [Serializable]
    public class CameraReelErrorResponse
    {
        public string message;
        public string reason;
    }
}
