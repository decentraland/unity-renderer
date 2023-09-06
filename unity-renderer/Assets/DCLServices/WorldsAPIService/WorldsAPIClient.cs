using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.Networking;

namespace DCLServices.WorldsAPIService
{
    public interface IWorldsAPIClient
    {
        UniTask<WorldsResponse.WorldsAPIResponse> SearchWorlds(string searchString, int pageNumber, int pageSize, CancellationToken ct);

        UniTask<WorldsResponse.WorldsAPIResponse> GetWorlds(int pageNumber, int pageSize, string filter = "", string sort = "", CancellationToken ct = default);

        UniTask<List<WorldsResponse.WorldInfo>> GetWorldsByNamesList(List<string> namesList, CancellationToken ct);
    }

    public class WorldsAPIClient : IWorldsAPIClient
    {
        private const string BASE_URL = "https://places.decentraland.org/api/worlds";

        private readonly IWebRequestController webRequestController;

        public WorldsAPIClient(IWebRequestController webRequestController)
        {
            this.webRequestController = webRequestController;
        }

        public async UniTask<WorldsResponse.WorldsAPIResponse> SearchWorlds(string searchString, int pageNumber, int pageSize, CancellationToken ct)
        {
            const string URL = BASE_URL + "?with_realms_detail=true&search={0}&offset={1}&limit={2}";
            var result = await webRequestController.GetAsync(string.Format(URL, searchString.Replace(" ", "+"), pageNumber * pageSize, pageSize), cancellationToken: ct, isSigned: true);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching worlds info:\n{result.error}");

            var response = Utils.SafeFromJson<WorldsResponse.WorldsAPIResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing world info:\n{result.downloadHandler.text}");

            if (response.data == null)
                throw new Exception($"No world info retrieved:\n{result.downloadHandler.text}");

            return response;
        }

        public async UniTask<WorldsResponse.WorldsAPIResponse> GetWorlds(int pageNumber, int pageSize, string filter = "", string sort = "", CancellationToken ct = default)
        {
            const string URL = BASE_URL + "?order_by={3}&order=desc&with_realms_detail=true&offset={0}&limit={1}&{2}";
            var result = await webRequestController.GetAsync(string.Format(URL, pageNumber * pageSize, pageSize, filter, sort), cancellationToken: ct, isSigned: true);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching worlds info:\n{result.error}");

            var response = Utils.SafeFromJson<WorldsResponse.WorldsAPIResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing word info:\n{result.downloadHandler.text}");

            if (response.data == null)
                throw new Exception($"No world info retrieved:\n{result.downloadHandler.text}");

            return response;
        }

        public async UniTask<List<WorldsResponse.WorldInfo>> GetWorldsByNamesList(List<string> namesList, CancellationToken ct)
        {
            if (namesList.Count == 0)
                return new List<WorldsResponse.WorldInfo>();

            var url = string.Concat(BASE_URL, "?");
            foreach (string name in namesList)
                url = string.Concat(url, $"names={name}&with_realms_detail=true&");

            var result = await webRequestController.GetAsync(url, cancellationToken: ct, isSigned: true);

            if (result.result != UnityWebRequest.Result.Success)
                throw new Exception($"Error fetching worlds info:\n{result.error}");

            var response = Utils.SafeFromJson<WorldsResponse.WorldsAPIResponse>(result.downloadHandler.text);

            if (response == null)
                throw new Exception($"Error parsing worlds info:\n{result.downloadHandler.text}");

            return response.data;
        }
    }
}
