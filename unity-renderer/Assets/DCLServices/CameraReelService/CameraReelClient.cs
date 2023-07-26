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
        UniTask<CameraReelResponse> GetScreenshot(string uuid, CancellationToken ct);
        UniTask<CameraReelResponse[]> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct);
        UniTask<CameraReelResponse> UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct);
        UniTask DeleteScreenshot(string uuid, CancellationToken ct);
    }

    public class CameraReelClient : ICameraReelClient
    {
        private const string IMAGE_BASE_URL = "https://camera-reel-service.decentraland.zone/api/images";
        private const string GALLERY_BASE_URL = "https://camera-reel-service.decentraland.zone/api/users";
        // https://camera-reel-service.decentraland.zone/api/users/0x05de05303eab867d51854e8b4fe03f7acb0624d9/images
        // https://camera-reel-service.decentraland.zone/api/users/0x05de05303eab867d51854e8b4fe03f7acb0624d9/images?limit=2&offset=0

        private readonly IWebRequestController webRequestController;

        public CameraReelClient(IWebRequestController webRequestController)
        {
            this.webRequestController = webRequestController;
        }

        public async UniTask<CameraReelResponse[]> GetScreenshotGallery(string userAddress, int limit, int offset, CancellationToken ct)
        {
            UnityWebRequest result = await webRequestController.GetAsync($"{GALLERY_BASE_URL}/{userAddress}/images?limit={limit}&offset={offset}", cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching user screenshots gallery :\n{result.error}");

            CameraReelResponses responseData = Utils.SafeFromJson<CameraReelResponses>(result.downloadHandler.text);

            if (responseData == null)
                throw new Exception($"Error parsing screenshots gallery response:\n{result.downloadHandler.text}");

            Debug.Log(responseData.currentImages);
            Debug.Log(responseData.maxImages);

            foreach (CameraReelResponse response in responseData.images)
                ResponseSanityCheck(response, result.downloadHandler.text);

            return responseData.images.ToArray();
        }
        public async UniTask<CameraReelResponse> UploadScreenshot(byte[] image, ScreenshotMetadata metadata, CancellationToken ct)
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("image", image, $"{metadata.dateTime}.jpg", "image/jpeg"),
                new MultipartFormDataSection("metadata", JsonUtility.ToJson(metadata)),
            };

            UnityWebRequest result = await webRequestController.PostAsync(IMAGE_BASE_URL, formData, cancellationToken: ct);
            return ParseScreenshotResponse(result, unSuccessResultMassage: "Error uploading screenshot");
        }

        public async UniTask DeleteScreenshot(string uuid, CancellationToken ct)
        {
            UnityWebRequest result = await webRequestController.DeleteAsync($"{IMAGE_BASE_URL}/{uuid}", isSigned: true, cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"error during deleting screenshot from the gallery:\n{result.error}");
        }

        public async UniTask<CameraReelResponse> GetScreenshot(string uuid, CancellationToken ct)
        {
            UnityWebRequest result = await webRequestController.GetAsync($"{IMAGE_BASE_URL}/{uuid}", cancellationToken: ct);
            return ParseScreenshotResponse(result, unSuccessResultMassage: "Error fetching screenshot image with metadata");
        }

        private static CameraReelResponse ParseScreenshotResponse(UnityWebRequest result, string unSuccessResultMassage)
        {
            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"{unSuccessResultMassage}:\n{result.error}");

            CameraReelResponse response = Utils.SafeFromJson<CameraReelResponse>(result.downloadHandler.text);

            ResponseSanityCheck(response, result.downloadHandler.text);
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
        public ulong currentImages;
        public ulong maxImages;
    }

    [Serializable]
    public class CameraReelResponse
    {
        public string id;
        public string url;
        public string thumbnailUrl;

        public ScreenshotMetadata metadata;
    }
}
