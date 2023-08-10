using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCLServices.EnvironmentProvider;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace DCLServices.CameraReelService
{
    public class CameraReelWebRequestClient : ICameraReelNetworkClient
    {
        private readonly IWebRequestController webRequestController;
        private readonly IEnvironmentProviderService environmentProviderService;

        private readonly string imageBaseURL;
        private readonly string userBaseURL;

        public CameraReelWebRequestClient(IWebRequestController webRequestController,
            IEnvironmentProviderService environmentProviderService)
        {
            this.webRequestController = webRequestController;
            this.environmentProviderService = environmentProviderService;

            imageBaseURL = $"https://camera-reel-service.decentraland.{(IsProdEnv() ? "org" : "zone")}/api/images";
            userBaseURL = $"https://camera-reel-service.decentraland.{(IsProdEnv() ? "org" : "zone")}/api/users";
        }

        private bool IsProdEnv() =>
            environmentProviderService.IsProd();

        public async UniTask<CameraReelStorageResponse> GetUserGalleryStorageInfoRequest(string userAddress, CancellationToken ct)
        {
            UnityWebRequest result = await webRequestController.GetAsync($"{userBaseURL}/{userAddress}", isSigned: true, cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching user gallery storage info :\n{result.error}");

            CameraReelStorageResponse responseData = Utils.SafeFromJson<CameraReelStorageResponse>(result.downloadHandler.text);

            if (responseData == null)
                throw new Exception($"Error parsing gallery storage info response:\n{result.downloadHandler.text}");

            return responseData;
        }

        public async UniTask<CameraReelResponses> GetScreenshotGalleryRequest(string userAddress, int limit, int offset, CancellationToken ct)
        {
            UnityWebRequest result = await webRequestController.GetAsync($"{userBaseURL}/{userAddress}/images?limit={limit}&offset={offset}", isSigned: true, cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching user screenshots gallery :\n{result.error}");

            CameraReelResponses responseData = Utils.SafeFromJson<CameraReelResponses>(result.downloadHandler.text);

            if (responseData == null)
                throw new Exception($"Error parsing screenshots gallery response:\n{result.downloadHandler.text}");

            foreach (CameraReelResponse response in responseData.images)
                ResponseSanityCheck(response, result.downloadHandler.text);

            return responseData;
        }

        public async UniTask<CameraReelUploadResponse> UploadScreenshotRequest(byte[] image, ScreenshotMetadata metadata, CancellationToken ct)
        {
            var formData = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("image", image, $"{metadata.dateTime}.jpg", "image/jpeg"),
                new MultipartFormDataSection("metadata", JsonUtility.ToJson(metadata)),
            };

            UnityWebRequest result = await webRequestController.PostAsync(imageBaseURL, formData, isSigned: true, cancellationToken: ct);

            CameraReelUploadResponse response = Utils.SafeFromJson<CameraReelUploadResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing screenshot response:\n{result.downloadHandler.text}");

            ResponseSanityCheck(response.image, result.downloadHandler.text);

            return response;
        }

        public async UniTask<CameraReelStorageResponse> DeleteScreenshotRequest(string uuid, CancellationToken ct)
        {
            UnityWebRequest result = await webRequestController.DeleteAsync($"{imageBaseURL}/{uuid}", isSigned: true, cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"error during deleting screenshot from the gallery:\n{result.error}");

            CameraReelStorageResponse response = Utils.SafeFromJson<CameraReelStorageResponse>(result.downloadHandler.text);

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
    public class CameraReelStorageResponse
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
