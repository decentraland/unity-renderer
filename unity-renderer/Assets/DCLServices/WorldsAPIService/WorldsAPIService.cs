using Cysharp.Threading.Tasks;
using DCL;
using DCL.Tasks;
using DCLServices.Lambdas;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.WorldsAPIService
{

    public interface IWorldsAPIService : IService
    {
        UniTask<(IReadOnlyList<WorldsResponse.WorldInfo> worlds, int total)> SearchWorlds(string searchText, int pageNumber, int pageSize, CancellationToken ct, bool renewCache = false);
        UniTask<(IReadOnlyList<WorldsResponse.WorldInfo> worlds, int total)> GetWorlds(int pageNumber, int pageSize, string filter = "", string sort = "", CancellationToken ct = default, bool renewCache = false);
    }

    public class WorldsAPIService : IWorldsAPIService, ILambdaServiceConsumer<WorldsResponse.WorldsAPIResponse>
    {
        private IWorldsAPIClient client;
        private readonly CancellationTokenSource disposeCts = new ();
        internal readonly Dictionary<string, LambdaResponsePagePointer<WorldsResponse.WorldsAPIResponse>> activeWorldsPagePointers = new ();

        public WorldsAPIService(IWorldsAPIClient client)
        {
            this.client = client;
        }
        public void Initialize() { }

        public async UniTask<(IReadOnlyList<WorldsResponse.WorldInfo> worlds, int total)> SearchWorlds(string searchText, int pageNumber, int pageSize, CancellationToken ct, bool renewCache = false)
        {
            WorldsResponse.WorldsAPIResponse worldsAPIResponse = await client.SearchWorlds(searchText, pageNumber, pageSize, ct);
            return (worldsAPIResponse.data, worldsAPIResponse.total);
        }

        public async UniTask<(IReadOnlyList<WorldsResponse.WorldInfo> worlds, int total)> GetWorlds(int pageNumber, int pageSize, string filter = "", string sort = "", CancellationToken ct = default,
            bool renewCache = false)
        {
            var createNewPointer = false;

            if (!activeWorldsPagePointers.TryGetValue($"{pageSize}_{filter}_{sort}", out var pagePointer)) { createNewPointer = true; }
            else if (renewCache)
            {
                pagePointer.Dispose();
                activeWorldsPagePointers.Remove($"{pageSize}_{filter}_{sort}");
                createNewPointer = true;
            }

            if (createNewPointer)
            {
                activeWorldsPagePointers[$"{pageSize}_{filter}_{sort}"] = pagePointer = new LambdaResponsePagePointer<WorldsResponse.WorldsAPIResponse>(
                    $"", // not needed, the consumer will compose the URL
                    pageSize, disposeCts.Token, this, TimeSpan.FromSeconds(30));
            }

            (WorldsResponse.WorldsAPIResponse response, bool _) = await pagePointer.GetPageAsync(pageNumber, ct, new Dictionary<string, string>(){{"filter", filter},{"sort", sort}});

            return (response.data, response.total);
        }

        public async UniTask<(WorldsResponse.WorldsAPIResponse response, bool success)> CreateRequest(string endPoint, int pageSize, int pageNumber, Dictionary<string,string> additionalData, CancellationToken ct = default)
        {
            var response = await client.GetWorlds(pageNumber, pageSize,additionalData["filter"],additionalData["sort"], ct);
            // Client will handle most of the error handling and throw if needed
            return (response, true);
        }

        public void Dispose()
        {
            disposeCts.SafeCancelAndDispose();
        }
    }
}
