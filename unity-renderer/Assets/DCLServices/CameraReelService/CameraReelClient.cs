using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using System;
using System.Threading;
using UI.InWorldCamera.Scripts;
using UnityEngine;
using UnityEngine.Networking;

namespace DCLServices.CameraReelService
{
    public interface ICameraReelClient
    {
        UniTask GetImage(string imageUUID, CancellationToken ct);

        UniTask GetImageMetadata(string imageUUID, CancellationToken ct);

        UniTask DeleteImage(string imageUUID, CancellationToken ct);

        UniTask UploadScreenshot(byte[] screenshot, byte[] metadata, CancellationToken ct);
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

        public async UniTask GetImage(string imageUUID, CancellationToken ct)
        {
            // Debug.Log("GET");
            //
            // return;

            // var client = new HttpClient();
            // var request = new HttpRequestMessage(HttpMethod.Get, "https://camera-reel-service.decentraland.zone/api/images/095c0e1a-63d5-4329-aad9-8e511704a971/metadata");
            // var response = await client.SendAsync(request);
            // response.EnsureSuccessStatusCode();
            // Console.WriteLine(await response.Content.ReadAsStringAsync());

            var url = $"{IMAGE_BASE_URL}/{imageUUID}";
            Debug.Log($"GETTING from {url}");
            var result = await webRequestController.GetAsync(url, cancellationToken: ct);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching place info:\n{result.error}");

            Debug.Log("RESULT");
            Debug.Log(result.downloadHandler.text);
            var response = Utils.SafeFromJson<CameraReelImageResponse>(result.downloadHandler.text);

            Debug.Log(response.id);
            Debug.Log(response.url);
            Debug.Log(response.metadata.userName);
            Debug.Log(response.metadata.userAddress);
            Debug.Log(response.metadata.dateTime);
            Debug.Log(response.metadata.realm);
            Debug.Log(response.metadata.scene.name);
            Debug.Log(response.metadata.scene.location.x);
            Debug.Log(response.metadata.scene.location.y);
            Debug.Log(response.metadata.visiblePeople[0].userName);
            Debug.Log(response.metadata.visiblePeople[0].userAddress);
            Debug.Log(response.metadata.visiblePeople[0].wearables[0]);
            Debug.Log(response.metadata.visiblePeople[0].wearables[1]);
            // var response = Utils.SafeFromJson<IHotScenesController.PlacesAPIGetParcelResponse>(result.downloadHandler.text);
            //
            // if (response == null)
            //     throw new Exception($"Error parsing place info:\n{result.downloadHandler.text}");
            //
            // if (response.ok == false)
            //     throw new NotAPlaceException(placeUUID);
            //
            // if (response.data == null)
            //     throw new Exception($"No place info retrieved:\n{result.downloadHandler.text}");
            //
            // return response.data;
        }

        public UniTask GetImageMetadata(string imageUUID, CancellationToken ct) =>
            throw new NotImplementedException();

        public UniTask DeleteImage(string imageUUID, CancellationToken ct) =>
            throw new NotImplementedException();

        public UniTask UploadScreenshot(byte[] screenshot, byte[] metadata, CancellationToken ct) =>
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
